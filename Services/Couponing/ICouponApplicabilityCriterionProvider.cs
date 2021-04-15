using Nwazet.Commerce.Descriptors.CouponApplicability;
using Orchard;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponCriterionProvider : IDependency {
        // Notes on the combination of these flags:
        // [true, true]: Everything is allowed. Backend users may add/remove/edit
        //   criteria from this provider. Criteria from this provider will be used
        //   in all computations for a coupon that includes them.
        // [true, false]: Backend users may add/remove/edit criteria from this
        //   provider. Criteria from this provider will be ignored in all computations
        //   for a coupon that includes them. When editing a coupon, criteria from this
        //   provider should be marked accordingly.
        // [false, true]: Backend users may not add/remove/edit criteria from this
        //   provider. Criteria from this provider will be used in all computations
        //   for a coupon that includes them. When editing a coupon, criteria from this
        //   provider should be marked accordingly.
        // [false, false]: Backend users may not add/remove/edit criteria from this
        //   provider. Criteria from this provider will be ignored in all computations
        //   for a coupon that includes them. When editing a coupon, criteria from this
        //   provider should be marked accordingly.


        /// <summary>
        /// Marks this criterion as manageable from backend.
        /// True: operators may add and edit criteria from this provider.
        /// False: operators may not add and edit criteria from this provider.
        ///   Any criteria from this provider that are already configured in a 
        ///   coupon will remain visible when editing the coupon, but will
        ///   not be interactive.
        /// </summary>
        /// <returns></returns>
        bool IsAvailableForConfiguration();
        /// <summary>
        /// Marks this criterion as "processable" for the end-users.
        /// True: criteria from this provider will be used when computing whether
        ///   a coupon can be added to a cart, and when computing whether the
        ///   coupon should afect prices.
        /// False: criteria from this provider will not be used.
        /// </summary>
        /// <returns></returns>
        bool IsAvailableForProcessing();
        /// <summary>
        /// Name for the provider used as key for its settings.
        /// </summary>
        string ProviderName { get; }
        LocalizedString ProviderDisplayName { get; }
    }
    public interface ICouponApplicabilityCriterionProvider : ICouponCriterionProvider {
        void Describe(DescribeCouponApplicabilityContext describe);
    }
    public interface ICouponLineApplicabilityCriterionProvider : ICouponCriterionProvider {
        void Describe(DescribeCouponLineApplicabilityContext describe);
    }
}
