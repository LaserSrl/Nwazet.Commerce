using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class MaxUsesCouponApplicabilityCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        // This condition, as is, may suffer of a race condition where several users
        // are attempting to use the alst available coupon at the same time.

        private readonly IUsedCouponsRepositoryService _usedCouponsRepositoryService;

        public MaxUsesCouponApplicabilityCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IUsedCouponsRepositoryService usedCouponsRepositoryService)
            : base(workContextAccessor, cacheManager, signals) {

            _usedCouponsRepositoryService = usedCouponsRepositoryService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string ProviderName =>
            "MaxUsesCouponApplicabilityCriteria";

        public override LocalizedString ProviderDisplayName =>
            T("Criterion on the number of times a coupon has been used in total.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Number of uses",
                    T("Number of times the coupon was used"),
                    T("Number of times the coupon was used"))
                .Element("Number of times the coupon was used by anyone",
                    T("Number of times the coupon was used by anyone"),
                    T("Number of times the coupon was used by anyone"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => T("The coupon has been used fewer than '{0}' times", (int)ctx.State.Value),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    PositiveIntegerValueForm.FormName);
        }

        private void ApplyCriterion(
            CouponApplicabilityCriterionContext context) {

            if (context.IsApplicable) {
                // get the value we should compare things to
                var sValue = (string)context.State.Value;
                var value = PositiveIntegerValueForm.ParseStateValue(sValue);
                if (value <= 0) {
                    // misconfiguration
                    context.ApplicabilityContext.Message =
                        T("Coupon code {0} is not valid", context.CouponRecord.Code);
                    context.IsApplicable = false;
                    context.ApplicabilityContext.IsApplicable = false;
                } else {
                    // value is positive
                    var pastUses = _usedCouponsRepositoryService
                        .Query()
                        .Where(cur =>
                            cur.CouponRecord_Id == context.ApplicabilityContext.Coupon.Id
                            // only count coupons that were actually spent
                            && cur.WasInvalid == false)
                        .Count();
                    if (pastUses < 0) {
                        // sanity check
                        context.ApplicabilityContext.Message =
                            T("Coupon code {0} is not valid", context.CouponRecord.Code);
                        context.IsApplicable = false;
                        context.ApplicabilityContext.IsApplicable = false;
                    } else {
                        // we can actually check
                        var result = pastUses < value;

                        if (!result) {
                            context.ApplicabilityContext.Message =
                                T("Coupon code {0} is not valid", context.CouponRecord.Code);
                        }
                        context.IsApplicable = result;
                        context.ApplicabilityContext.IsApplicable = result;
                    }
                }
            }
        }
    }
}
