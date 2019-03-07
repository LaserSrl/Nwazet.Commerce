using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CriteriaAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> Criteria { get; set; }
    }
}
