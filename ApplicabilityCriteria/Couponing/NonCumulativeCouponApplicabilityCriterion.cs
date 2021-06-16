using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Linq;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class NonCumulativeCouponApplicabilityCriterion
         : BaseCouponApplicabilityCriterion, ICouponApplicabilityCriterion {

        public NonCumulativeCouponApplicabilityCriterion(
           IWorkContextAccessor workContextAccessor,
           ICacheManager cacheManager,
           ISignals signals)
           : base(workContextAccessor, cacheManager, signals) {

        }

        public override string ProviderName =>
            "NonCumulativeCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName =>
            T("Coupons in the shopping cart are not cumulative");

        
        public override void CanBeAdded(CouponApplicabilityContext context) {
            if (EvaluateCriterion()) {
                if (context.IsApplicable) {
                    if (context.ShoppingCart?.PriceAlterations != null
                        && context.ShoppingCart.PriceAlterations.Any()) {
                        // coupons are not cumulative
                        context.IsApplicable = false;
                        context.Message = T("Coupons are not cumulative, coupon code {0} cannot be used.", context.Coupon.Code);
                    }
                }
            }
        }

        public override void CanBeProcessed(CouponApplicabilityContext context) {
            if (EvaluateCriterion()) {
                if (context.IsApplicable) {
                    if (context.ShoppingCart?.PriceAlterations != null
                        && context.ShoppingCart.PriceAlterations.Count() > 1) {
                        // coupons are not cumulative
                        context.IsApplicable = false;
                        context.Message = T("Coupons are not cumulative, coupon code {0} cannot be used.", context.Coupon.Code);
                    }
                }
            }
        }
    }
}
