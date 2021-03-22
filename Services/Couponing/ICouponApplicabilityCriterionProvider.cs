using Nwazet.Commerce.Descriptors.CouponApplicability;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponApplicabilityCriterionProvider : IDependency {
        void Describe(DescribeCouponApplicabilityContext describe);
    }
}
