using Nwazet.Commerce.ApplicabilityCriteria;
using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.Descriptors.ApplicabilityCriterion {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CriterionContext {
        public CriterionContext() {
            Tokens = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }

        public ApplicabilityContext ApplicabilityContext { get; set; }
        public bool IsApplicable { get; set; }

        public FlexibleShippingMethodRecord FlexibleShippingMethodRecord { get; set; }
    }
}
