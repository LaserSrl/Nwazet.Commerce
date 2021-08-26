using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Recipes.Providers.Executors {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponStep : RecipeExecutionStep {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly INotifier _notifier;

        private readonly Lazy<CultureInfo> _cultureInfo;

        public CouponStep(
            RecipeExecutionLogger logger,
            IWorkContextAccessor workContextAccessor,
            ICouponRepositoryService couponRepositoryService,
            INotifier notifier) : base(logger) {

            _workContextAccessor = workContextAccessor;
            _couponRepositoryService = couponRepositoryService;
            _notifier = notifier;

            _cultureInfo = new Lazy<CultureInfo>(() =>
                CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));

            T = NullLocalizer.Instance;
        }

        public Localizer T;
        public override string Name => "Coupons";

        public override void Execute(RecipeExecutionContext context) {
            // parse XML
            var coupons = ParseCoupons(context);
            // create records
            if (coupons != null) {
                foreach (var coupon in coupons) {
                    // verify that the coupon is unique
                    if (!_couponRepositoryService.Validate(coupon)) {
                        var code = coupon.Code;
                        var variant = 1;
                        coupon.Code = code + variant.ToString();
                        while (!_couponRepositoryService.Validate(coupon)) {
                            variant++;
                            coupon.Code = code + variant.ToString();
                        }
                        _notifier.Warning(T("Coupon with code {0} was already configured. We imported a new coupon with code {1}", code, coupon.Code));
                    }
                    coupon.Id = _couponRepositoryService.CreateRecord(coupon);
                    if (coupon.ApplicabilityCriteria.Any() || coupon.LineCriteria.Any()) {
                        // to add criteria, we need the record for the coupon
                        var created = _couponRepositoryService.Get(coupon.Id);
                        foreach (var criterion in coupon.ApplicabilityCriteria) {
                            var cRecord = new CouponApplicabilityCriterionRecord {
                                Category = criterion.Category,
                                Type = criterion.Type,
                                State = criterion.State,
                                Description = criterion.DisplayText
                            };
                            created.Record.ApplicabilityCriteria.Add(cRecord);
                        }
                        foreach (var criterion in coupon.LineCriteria) {
                            var cRecord = new CouponLineCriterionRecord {
                                Category = criterion.Category,
                                Type = criterion.Type,
                                State = criterion.State,
                                Description = criterion.DisplayText
                            };
                            created.Record.LineCriteria.Add(cRecord);
                        }
                    }

                }
            }
        }

        private IEnumerable<Coupon> ParseCoupons(RecipeExecutionContext context) {
            return context.RecipeStep.Step.Elements().Select(xel => {
                var coupon = new Coupon();
                coupon.Name = xel.Attribute("Name").Value;
                coupon.Code = xel.Attribute("Code").Value;
                int prio = 0;
                int.TryParse(xel.Attribute("Priority").Value, out prio);
                coupon.Priority = prio;
                bool published = false;
                bool.TryParse(xel.Attribute("Published").Value, out published);
                coupon.Published = published;
                CouponType cType = CouponType.Amount;
                Enum.TryParse(xel.Attribute("CouponType").Value, out cType);
                coupon.CouponType = cType;
                // Value is a string, but it represents a number: we exported it
                // in the invariant culture
                var val = xel.Attribute("Value").Value;
                decimal value = 0.0m;
                decimal.TryParse(xel.Attribute("Value").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                coupon.Value = value.ToString(_cultureInfo.Value);
                // Applicability Criteria
                foreach (var cel in xel.Elements("ApplicabilityCriterion")) {
                    coupon.ApplicabilityCriteria.Add(new CouponApplicabilityCriterionEntry() {
                        Category = cel.Attribute("Category").Value,
                        Type = cel.Attribute("Type").Value,
                        DisplayText = cel.Attribute("DisplayText").Value,
                        State = cel.Attribute("State").Value
                    });
                }
                // Line Criteria
                foreach (var cel in xel.Elements("LineCriterion")) {
                    coupon.LineCriteria.Add(new CouponApplicabilityCriterionEntry() {
                        Category = cel.Attribute("Category").Value,
                        Type = cel.Attribute("Type").Value,
                        DisplayText = cel.Attribute("DisplayText").Value,
                        State = cel.Attribute("State").Value
                    });
                }
                return coupon;
            });
        }
    }
}
