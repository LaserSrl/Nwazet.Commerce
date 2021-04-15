using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Orchard;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    /// <summary>
    /// Implementations will provide specialized tests to make sure coupons can
    /// be used, added to a cart and so on. These implementations represent 
    /// default tests that should be executed for all coupons. Look at 
    /// implementing ICouponApplicabilityCriterionProvider for the configurable 
    /// criteria.
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
        /// <remarks>This is a separate method from CanBeAdded because they are to
        /// be called at two different times nad may need to behave differently, 
        /// because the context can be such that a given coupon cannot be added
        /// to a transaction, but may allow to be processed and "used" if it's already
        /// in place.</remarks>
        void CanBeProcessed(CouponApplicabilityContext context);

        /// <summary>
        /// Tells whether this criterion should be considered or not. Note that 
        /// implementations of this interface affect all coupons in a tenant, so
        /// an enabled criterion will always run.
        /// </summary>
        /// <returns></returns>
        bool EvaluateCriterion();
        /// <summary>
        /// Name for the provider used as key for its settings.
        /// </summary>
        string ProviderName { get; }
        LocalizedString ProviderDisplayName { get; }
    }
}
