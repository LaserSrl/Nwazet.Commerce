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
            T("Pevent adding and using coupons that would cause negative price for a line or for the whole cart.");

        public override void CanBeAdded(CouponApplicabilityContext context) {
            TestForNegative(context);
        }
        public override void CanBeProcessed(CouponApplicabilityContext context) {
            TestForNegative(context);
        }

        private void TestForNegative(CouponApplicabilityContext context) {
            if (EvaluateCriterion() && context.IsApplicable) {
                switch (context.Coupon.CouponType) {
                    case Models.Couponing.CouponType.Percent:
                        // a percent change on a value can only make it negative if it's more than
                        // 100%. Note that, on principle, we could configure a coupon that causes 
                        // a price "reduction" of more than 100%, if the coupon only applied to
                        // part of a line: e.g. a coupon for 120% of the price of a product, applied
                        // to a line that has at least two of those products, is valid.
                        break;
                    case Models.Couponing.CouponType.Amount:
                        break;
                    case Models.Couponing.CouponType.CartAmount:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
