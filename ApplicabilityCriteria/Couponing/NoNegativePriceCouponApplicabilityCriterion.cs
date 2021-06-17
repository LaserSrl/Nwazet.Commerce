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
            
        }
    }
}
