using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class NullCouponUserIdentifierProvider : ICouponUserIdentifierProvider {
        public int Priority => int.MinValue;

        public string GetAdditionalUserIdentifier(CouponApplicabilityContext context) {
            return string.Empty;
        }

        public string GetIdentifierType(CouponApplicabilityContext context) {
            return string.Empty;
        }
    }
}
