using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponEvaluationService : IDependency {
        bool CanProcess(CouponApplicabilityContext context);
        bool CanApply(CouponApplicabilityContext context);

        /// <summary>
        /// Compute the contribution of a coupon to a cart price. Note that this
        /// method will not test whether the coupon is valid for the cart/context.
        /// </summary>
        /// <param name="coupon">We are computing stuff for this coupon</param>
        /// <param name="cart">This is the cart</param>
        /// <param name="previousValues">Values computed for coupons with higher priority</param>
        /// <param name="value">The value contribution of this coupon on the line</param>
        /// <returns></returns>
        bool TryCouponCartValue(
            CouponRecord coupon,
            IShoppingCart cart,
            IEnumerable<decimal> previousValues,
            out decimal value);

        /// <summary>
        /// Compute the contribution of a coupon to a cart line. Note that this
        /// method will not test whether the coupon is valid for the cart/context.
        /// </summary>
        /// <param name="coupon">We are computing stuff for this coupon</param>
        /// <param name="cart">This is the shopping cart</param>
        /// <param name="cartLine">This is the specific line we are doing our computations on</param>
        /// <param name="previousValues">Values computed for coupons with higher priority</param>
        /// <param name="value">The value contribution of this coupon on the line</param>
        /// <returns></returns>
        bool TryCouponLineValue(
            CouponRecord coupon,
            IShoppingCart cart,
            ShoppingCartQuantityProduct cartLine,
            IEnumerable<decimal> previousValues,
            out decimal value);

        bool TryCouponCartValue(
            CartPriceAlterationContext context,
            out decimal value);

        bool TryCouponLineValue(
            LinePriceAlterationContext context,
            out decimal value);
    }
}
