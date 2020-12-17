using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FilterEditors.Forms;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace Nwazet.Commerce.Filters {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class ProductPriceWithVatFilter
        : BaseDecimalFilterProvider<ProductVatConfigurationPartRecord> {

        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly IContentManager _contentManager;

        protected override string FilterCategory => "ProductPart";
        protected override LocalizedString FilterName => T("Product");
        protected override LocalizedString FilterDescription => T("Product");

        protected override string FilterElementType => "ProductPriceWithVat";
        protected override LocalizedString FilterElementName => T("Product Price with VAT");
        protected override LocalizedString FilterElementDescription => T("Product Price after VAT computations.");

        protected override string FormName => "ProductPriceWithVatFilterForm";
        protected override string DisplayProperty => T("Price with VAT").Text;

        // we will override the code where this property is used in the base class
        protected override string PropertyName => "";

        public ProductPriceWithVatFilter(
            IContentManager contentManager,
            IVatConfigurationService vatConfigurationService) 
            : base() {

            _vatConfigurationService = vatConfigurationService;
            _contentManager = contentManager;

            _vatRates = new Dictionary<int, Dictionary<int, decimal>>();
        }

        private Dictionary<int, Dictionary<int, decimal>> _vatRates;

        private Dictionary<int, decimal> GetRates(TerritoryInternalRecord destination) {
            if (!_vatRates.ContainsKey(destination.Id)) {
                var rates = new Dictionary<int, decimal>();
                var allConfigs = _contentManager.Query<VatConfigurationPart>().List();
                foreach (var vc in allConfigs) {
                    rates.Add(vc.Id, _vatConfigurationService.GetRate(vc, destination));
                }
                var defaultRate = _vatConfigurationService
                    .GetRate(_vatConfigurationService.GetDefaultCategory(), destination);
                rates.Add(0, defaultRate);
                _vatRates.Add(destination.Id, rates);
            }
            return _vatRates[destination.Id];
        }

        public override void ApplyFilter(FilterContext context) {
            decimal dvalue, dmin, dmax;
            dvalue = FromStateValue(context.State.Value);
            dmin = FromStateValue(context.State.Min);
            dmax = FromStateValue(context.State.Max);
            var op = (NumericOperator)Enum.Parse(
                typeof(NumericOperator), Convert.ToString(context.State.Operator));

            Action<IAliasFactory> alias;
            Action<IHqlExpressionFactory> predicate;
            // TODO: make the destination configurable as a token in the context/form
            var defaultDestination = _vatConfigurationService.GetDefaultDestination();
            if (defaultDestination == null) {
                // the configuration is telling that the prices on the frontend should be 
                // "before tax"
                alias = af => af.ContentPartRecord<PricePartRecord>();
                // I use PricePartRecord rather than base my conputations on the
                // properties of ProductPartVersionRecord, because the methods like
                //   ef3.LtProperty("DiscountPrice", "Price")
                // rather than doing 
                //   record.DiscountPrice < record.Price
                // do 
                //   record.DiscountPrice < "Price"
                predicate = FilterHelper
                    .GetFilterPredicateNumeric(op, "EffectiveUnitPrice", dvalue, dmin, dmax);
                context.Query.Where(alias, predicate);
            } else {
                // for each existing vat configuration, get the rate for the configured default
                // territory
                var rates = GetRates(defaultDestination);

                alias = af => af.ContentItem(); // ContentPartRecord<PricePartRecord>();

                predicate = GetFilterPredicateNumeric(op, dvalue, dmin, dmax, 
                    rates, context.QueryPartRecord.VersionScope);
                context.Query.Where(alias, predicate);
            }
            
        }

        public Action<IHqlExpressionFactory> GetFilterPredicateNumeric(
            NumericOperator op, decimal? value, decimal? min, decimal? max,
            Dictionary<int, decimal> rates,
            QueryVersionScopeOptions versionScope) {
            decimal dmin, dmax;
            if (op == NumericOperator.Between || op == NumericOperator.NotBetween) {
                dmin = min.HasValue ? min.Value : 0m;
                dmax = max.HasValue ? max.Value : 0m;
            } else {
                dmin = dmax = value.HasValue ? value.Value : 0m;
            }

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("SELECT cir.Id as Id");
            queryBuilder.AppendLine("FROM Nwazet.Commerce.Models.PricePartRecord as ppr");
            queryBuilder.AppendLine("JOIN ppr.ContentItemRecord as cir");
            queryBuilder.AppendLine("JOIN ppr.ContentItemVersionRecord as civr");
            
            queryBuilder.AppendLine(",Nwazet.Commerce.Models.ProductVatConfigurationPartRecord as pvcpr");

            queryBuilder.AppendLine("WHERE");
            switch (versionScope) {
                case QueryVersionScopeOptions.Published:
                    queryBuilder.AppendLine("civr.Published=1");
                    break;
                case QueryVersionScopeOptions.Latest:
                    queryBuilder.AppendLine("civr.Latest=1");
                    break;
                case QueryVersionScopeOptions.Draft:
                    queryBuilder.AppendLine("civr.Latest=1 AND civr.Published=0");
                    break;
                default:
                    queryBuilder.AppendLine("civr.Published=1");
                    break;
            }
            queryBuilder.AppendLine("AND pvcpr.Id = cir.Id");

            string testFormat = "";
            string nullFormat = "";
            switch (op) {
                case NumericOperator.LessThan:
                    testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) < {3}))";
                    nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) < {3}))";
                    break;
                case NumericOperator.LessThanEquals:
                    testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) <= {3}))";
                    nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) <= {3}))";
                    break;
                case NumericOperator.Equals:
                    if (dmin == dmax) {
                        testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) = {3}))";
                        nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) = {3}))";
                    } else {
                        testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) >= {2} and (ppr.EffectiveUnitPrice * {1}) <= {3}))";
                        nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) >= {2} and (ppr.EffectiveUnitPrice * {1}) <= {3}))";
                    }
                    break;
                case NumericOperator.NotEquals:
                    if (dmin == dmax) {
                        testFormat = "(pvcpr.VatConfiguration.Id = {0} and not((ppr.EffectiveUnitPrice * {1}) = {2}))";
                        nullFormat = "(pvcpr.VatConfiguration IS NULL and not((ppr.EffectiveUnitPrice * {1}) = {2}))";
                    } else {
                        testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) < {2} or (ppr.EffectiveUnitPrice * {1}) > {3}))";
                        nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) < {2} or (ppr.EffectiveUnitPrice * {1}) > {3}))";
                    }
                    break;
                case NumericOperator.GreaterThan:
                    testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) > {2}))";
                    nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) > {2}))";
                    break;
                case NumericOperator.GreaterThanEquals:
                    testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) >= {2}))";
                    nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) >= {2}))";
                    break;
                case NumericOperator.Between:
                    testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) >= {2} and (ppr.EffectiveUnitPrice * {1}) <= {3}))";
                    nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) >= {2} and (ppr.EffectiveUnitPrice * {1}) <= {3}))";
                    break;
                case NumericOperator.NotBetween:
                    testFormat = "(pvcpr.VatConfiguration.Id = {0} and ((ppr.EffectiveUnitPrice * {1}) < {2} or (ppr.EffectiveUnitPrice * {1}) > {3}))";
                    nullFormat = "(pvcpr.VatConfiguration IS NULL and ((ppr.EffectiveUnitPrice * {1}) < {2} or (ppr.EffectiveUnitPrice * {1}) > {3}))";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var tests = new List<string>(rates.Count());
            foreach (var rate in rates) {
                tests.Add(
                    string.Format(
                        testFormat,
                        rate.Key, 
                        (1m + rate.Value).ToString(CultureInfo.InvariantCulture), 
                        dmin.ToString(CultureInfo.InvariantCulture), 
                        dmax.ToString(CultureInfo.InvariantCulture)));
            }
            // handle default vat configuration
            tests.Add(
                string.Format(
                    testFormat,
                    0,
                    (1m + rates[0]).ToString(CultureInfo.InvariantCulture),
                    dmin.ToString(CultureInfo.InvariantCulture),
                    dmax.ToString(CultureInfo.InvariantCulture)));
            tests.Add(
                string.Format(
                    nullFormat,
                    0,
                    (1m + rates[0]).ToString(CultureInfo.InvariantCulture),
                    dmin.ToString(CultureInfo.InvariantCulture),
                    dmax.ToString(CultureInfo.InvariantCulture)));
            queryBuilder.AppendLine("AND (" + string.Join(" or ", tests) + ")");

            return x => x.InSubquery("Id", queryBuilder.ToString(), new Dictionary<string, object>());
        }
        
    }
}
