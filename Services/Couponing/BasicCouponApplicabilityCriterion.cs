using Nwazet.Commerce.Extensions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class BasicCouponApplicabilityCriterion : ICouponApplicabilityCriterion {
        // This implementation provides general etsts that are ok for any and every coupon.
        // More specialized criteria should have their own implementations that only check
        // their single condition. For example, this is not the place to check that the cart
        // isn't empty, as on principle there may be coupons that can be added to empty carts.
        public BasicCouponApplicabilityCriterion() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void CanBeAdded(CouponApplicabilityContext context) {
            
            if (context.IsApplicable) {
                if (context.ShoppingCart?.PriceAlterations != null
                        && context.ShoppingCart.PriceAlterations.Any(cpa =>
                            CouponingUtilities.CouponAlterationType.Equals(cpa.AlterationType)
                            && context.Coupon.Code.Equals(cpa.Key, StringComparison.InvariantCultureIgnoreCase))) {
                    // is this coupon already applied?
                    // can't apply the same coupon twice
                    context.IsApplicable = false;
                    context.Message = T("Coupon code {0} is already in use.", context.Coupon.Code);
                }
            }
        }

        public void CanBeProcessed(CouponApplicabilityContext context) {
            
        }
    }
}
