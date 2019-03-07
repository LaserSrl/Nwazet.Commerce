using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;

namespace Nwazet.Commerce.Descriptors.ApplicabilityCriterion {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class ApplicabilityCriterionDescriptor {
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Action<CriterionContext> TestCriterion { get; set; }
        public string Form { get; set; }
        public Func<CriterionContext, LocalizedString> Display { get; set; }
    }
}
