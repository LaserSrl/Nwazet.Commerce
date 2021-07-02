using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Linq;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class NoNegativePriceCouponApplicabilityCriterion
        : BaseCouponApplicabilityCriterion, ICouponApplicabilityCriterion {
        // This implementation verifies that the coupon doesn't cause any line, or
        // the cart total, to become negative.

        public NoNegativePriceCouponApplicabilityCriterion(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals)
            : base(workContextAccessor, cacheManager, signals) {
            
        }

        public override string ProviderName =>
            "NoNegativePriceCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName =>
            T("Prevent adding and using coupons that would cause negative price for a line or for the whole cart.");

        public override void PostCanBeAdded(CouponPostApplicabilityContext context) {
            if (EvaluateCriterion() && context.IsApplicable) {
                Evaluate(context);
            }
        }

        public override void PostCanBeProcessed(CouponPostApplicabilityContext context) {
            if (EvaluateCriterion() && context.IsApplicable) {
                Evaluate(context);
            }
        }

        private void Evaluate (
            CouponPostApplicabilityContext context) {
            // fail this coupon if any line ends up having a value <= 0 
            context.IsApplicable &= !context.ContextsForLines()
                .Any(lctx => 0 > (lctx.BaseLinePrice 
                    // Values are negative already
                    + lctx.CouponValues.Sum(cv => cv.Value)));
            // fail this coupon if the cart ends up having a total <= 0 
            context.IsApplicable &= !(0 > (context.BaseCartSubtotal
                // Values are negative already
                + context.CouponValues.Sum(cv => cv.Value)));
        }
    }
}
