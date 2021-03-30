using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponUserIdentifierProvider : IDependency {

        int Priority { get; }

        string GetAdditionalUserIdentifier(CouponApplicabilityContext context);
        string GetIdentifierType(CouponApplicabilityContext context);
    }
}
