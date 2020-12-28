using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    /// <summary>
    /// Implementations will provide specialized tests to make sure coupons can
    /// be used, added to a cart and so on.
    /// In some implementations one method may call the other as a prerequisite.
    /// </summary>
    public interface ICouponApplicabilityCriterion : IDependency {
        /// <summary>
        /// can the coupon in the context be added to the cart?
        /// </summary>
        /// <param name="context"></param>
        void CanBeAdded(CouponApplicabilityContext context);
        /// <summary>
        /// Can the coupon in the context be processed?
        /// </summary>
        /// <param name="context"></param>
        void CanBeProcessed(CouponApplicabilityContext context);
    }
}
