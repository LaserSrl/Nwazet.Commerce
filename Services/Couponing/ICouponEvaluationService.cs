using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
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
    }
}
