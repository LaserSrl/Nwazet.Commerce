using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CriterionEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public ApplicabilityCriterionDescriptor Criterion { get; set; }
        public dynamic Form { get; set; }
    }
}
