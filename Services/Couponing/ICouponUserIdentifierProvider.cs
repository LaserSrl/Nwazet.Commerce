using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponUserIdentifierProvider : IDependency {
        /// <summary>
        /// Priority for the provider. Higher priorities win.
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// Gets a value to use as user identifier. Returning null will cause the 
        /// provider to be skipped.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetAdditionalUserIdentifier(CouponApplicabilityContext context);
        /// <summary>
        /// Unique string to identify the type for the provider.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetIdentifierType(CouponApplicabilityContext context);
    }
}
