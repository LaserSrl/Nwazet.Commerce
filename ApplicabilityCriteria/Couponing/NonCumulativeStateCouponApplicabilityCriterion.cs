using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
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
    public class NonCumulativeStateCouponApplicabilityCriterion
          : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        public NonCumulativeStateCouponApplicabilityCriterion(
           IWorkContextAccessor workContextAccessor,
           ICacheManager cacheManager,
           ISignals signals)
           : base(workContextAccessor, cacheManager, signals) {

        }
        public override string ProviderName => "NonCumulativeStateCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName => T("Criteria non cumulative state");


        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();
            describe
                .For("Coupon", T("Non cumulative state"), T("Non cumulative State"))
                .Element("Non cumulative State",
                    T("Non cumulative State"),
                    T("If the criteria is present the coupon is not cumulative with other coupons"),
                    (ctx) => ApplyCriteria(ctx, T("Coupon {0} cannot be cumulated.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriteria(ctx, T("Coupon {0} cannot be cumulated.", ctx.CouponRecord.Code)),
                    (ctx) => T("Non cumulative State"),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null);
        }

        public void ApplyCriteria(CouponApplicabilityCriterionContext context,
            LocalizedString failureMessage) {
            // Use outerCriterion to negate the test, so we can easily do
            // true/false
            if (context.IsApplicable) {
                var coupons = context.ApplicabilityContext.ShoppingCart?.PriceAlterations != null ?
                    context.ApplicabilityContext.ShoppingCart.PriceAlterations.Where(c => c.AlterationType == CouponingUtilities.CouponAlterationType) :
                    new List<CartPriceAlteration>();

                // when the applicability condition must be true
                // there are no coupons
                var result = !coupons.Any() ||
                    // if there is only one coupon and it is me
                    (coupons.Count()==1 &&
                    context.ApplicabilityContext.ShoppingCart.PriceAlterations.First().Key == context.CouponRecord.Code);
                //verify is current coupon
                if (!result) {
                    context.ApplicabilityContext.Message = failureMessage;
                }
                context.IsApplicable = result;
                context.ApplicabilityContext.IsApplicable = result;
            }
        }
    }
}
