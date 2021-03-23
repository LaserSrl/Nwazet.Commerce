using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponContext {

        public CouponContext() {
            Tokens = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
        public bool IsApplicable { get; set; }
        public CouponRecord CouponRecord { get; set; }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponContext<TInner>
        : CouponContext
        where TInner : CouponApplicabilityContext {
        public TInner ApplicabilityContext { get; set; }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicabilityCriterionContext 
        : CouponContext<CouponApplicabilityContext> {
    }


    [OrchardFeature("Nwazet.Couponing")]
    public class CouponLineCriterionContext
        : CouponContext<CouponLineApplicabilityContext> {
    }
}
