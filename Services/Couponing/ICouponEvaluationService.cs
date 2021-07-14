using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Orchard;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponEvaluationService : IDependency {
        bool CanProcess(CouponApplicabilityContext context);
        bool CanApply(CouponApplicabilityContext context);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryCouponCartValue(
            CartPriceAlterationContext context,
            out decimal value);
        /// <summary>
        /// Compute the contribution of a coupon to a cart line. Note that this
        /// method will not test whether the coupon is valid for the cart/context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value">The value contribution of this coupon on the line</param>
        /// <returns></returns>
        bool TryCouponLineValue(
            LinePriceAlterationContext context,
            out decimal value);
    }
}
