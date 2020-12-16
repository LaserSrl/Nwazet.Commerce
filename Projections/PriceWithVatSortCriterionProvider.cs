using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Providers.SortCriteria;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Projections {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class PriceWithVatSortCriterionProvider : ISortCriterionProvider {

        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly IContentManager _contentManager;

        public PriceWithVatSortCriterionProvider(
            IContentManager contentManager,
            IVatConfigurationService vatConfigurationService) {

            _vatConfigurationService = vatConfigurationService;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;

            _vatRates = new Dictionary<int, Dictionary<int, decimal>>();
        }

        private Dictionary<int, Dictionary<int, decimal>> _vatRates;

        public Localizer T { get; set; }

        private Dictionary<int, decimal> GetRates(TerritoryInternalRecord destination) {
            if (!_vatRates.ContainsKey(destination.Id)) {
                var rates = new Dictionary<int, decimal>();
                var allConfigs = _contentManager.Query<VatConfigurationPart>().List();
                foreach (var vc in allConfigs) {
                    rates.Add(vc.Id, _vatConfigurationService.GetRate(vc, destination));
                }
                _vatRates.Add(destination.Id, rates);
            }
            return _vatRates[destination.Id];
        }

        public void Describe(DescribeSortCriterionContext describe) {
            describe.For("ProductPart", T("Product"), T("Product"))
                .Element("SortByPriceWithVat",
                    T("Sort by price"),
                    T("Sort based on the price after VAT is applied."),
                    context => ApplyFilter(context),
                    context => DisplaySortCriterion(context),
                    SortCriterionFormProvider.FormName);
        }

        public LocalizedString DisplaySortCriterion(SortCriterionContext context) {
            bool ascending = Convert.ToBoolean(context.State.Sort);
            if (ascending)
                return T("Sort based on the price after VAT is applied, ascending.");
            else
                return T("Sort based on the price after VAT is applied, descending.");
        }

        private void ApplyFilter(SortCriterionContext context) {

            bool ascending = Convert.ToBoolean(context.State.Sort);
            

            // TODO: make the destination configurable as a token in the context/form?
            var defaultDestination = _vatConfigurationService.GetDefaultDestination();
            if (defaultDestination == null) {
                // the configuration is telling that the prices on the frontend should be 
                // "before tax"
                Action<IAliasFactory> alias = af => af
                    .ContentPartRecord<PricePartRecord>();
                context.Query = ascending
                    ? context.Query
                        .OrderBy(alias,
                        sf => sf.Asc("EffectiveUnitPrice"))
                    : context.Query
                        .OrderBy(alias,
                        sf => sf.Desc("EffectiveUnitPrice"));

            } else {

                // for each existing vat configuration, get the rate for the configured default
                // territory
                var rates = GetRates(defaultDestination);
                var computedPropName = "g" + //don't start with a digit. "g" is just a character
                    Guid.NewGuid()
                        .ToString()
                        .Split('-')[0]
                    + "PriceWithVat";
                Action<IAliasFactory> alias = af => af
                    .ContentPartRecord<ProductVatConfigurationPartRecord>()
                    .ContentPartRecord<PricePartRecord>();

                context.Query = ascending
                    ? context.Query
                        .OrderBy(alias,
                        sf => sf.Asc("EffectiveUnitPrice",
                            SelectOps(rates, computedPropName),
                            OrderByOps(rates, computedPropName)
                            ))
                    : context.Query
                        .OrderBy(alias,
                        sf => sf.Desc("EffectiveUnitPrice",
                            SelectOps(rates, computedPropName),
                            OrderByOps(rates, computedPropName)
                            ));
            }
        }

        private string SelectOps(Dictionary<int, decimal> rates,
            string computedName) {
            // the default alias for ContentPartRecords is defined by 
            // DefaultHqlQuery.ParthToAlias(string path)
            // where path = contentPartRecordType.Name
            if (rates.Any()) {
                return MySortFormula(rates);
                //var pvcprAlias = PathToAlias(typeof(ProductVatConfigurationPartRecord).Name);
                //var pprAlias = PathToAlias(typeof(PricePartRecord).Name);
                //var queryBuilder = new StringBuilder();
                //foreach (var rate in rates) {
                //    // open nested case/when/else
                //    queryBuilder.Append(" case " + pvcprAlias + ".VatConfiguration.Id when " + rate.Key);
                //    queryBuilder.Append(" then " + pprAlias + ".EffectiveUnitPrice * " + (1m + rate.Value));
                //    queryBuilder.Append(" else");
                //}
                //queryBuilder.Append(" 0");
                //foreach (var rate in rates) {
                //    // close nested case/when/else
                //    queryBuilder.Append(" end");
                //}
                //queryBuilder.Append(" as " + computedName);
                //return queryBuilder.ToString();
            }
            return null;
            //return
            //    @"case vcpr.Id when 534 
		          //  then ppr.EffectiveUnitPrice * 1.10 
		          //  else case vcpr.Id when 535
			         //   then ppr.EffectiveUnitPrice * 1.22
			         //   else case vcpr.Id when 536
				        //    then ppr.EffectiveUnitPrice * 1.04
				        //    else 0
				        //    end
			         //   end
		          //  end as PrezzoIvato";
        }

        private string OrderByOps(Dictionary<int, decimal> rates,
            string computedName) {
            if (rates.Any()) {
                
                return MySortFormula(rates);
                //return computedName;
            }
            return null;
            //return
            //    @"PrezzoIvato";
        }

        // copied from DefaultHqlQuery
        private string PathToAlias(string path) {
            if (String.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException("Path can't be empty");
            }

            return Char.ToLower(path[0], CultureInfo.InvariantCulture) + path.Substring(1);
        }

        private string MySortFormula(Dictionary<int, decimal> rates) {
            if (rates.Any()) {
                var pvcprAlias = PathToAlias(typeof(ProductVatConfigurationPartRecord).Name);
                var pprAlias = PathToAlias(typeof(PricePartRecord).Name);
                var queryBuilder = new StringBuilder();
                foreach (var rate in rates) {
                    // open nested case/when/else
                    queryBuilder.Append(" case " + pvcprAlias + ".VatConfiguration.Id when " + rate.Key);
                    queryBuilder.Append(" then " + pprAlias + ".EffectiveUnitPrice * " + (1m + rate.Value));
                    queryBuilder.Append(" else");
                }
                queryBuilder.Append(" 0");
                foreach (var rate in rates) {
                    // close nested case/when/else
                    queryBuilder.Append(" end");
                }
                return queryBuilder.ToString();
            }
            return null;
        }
    }
}
