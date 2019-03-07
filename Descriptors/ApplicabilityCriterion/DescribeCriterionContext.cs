using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Descriptors.ApplicabilityCriterion {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class DescribeCriterionContext {
        private readonly Dictionary<string, DescribeCriterionFor> _describes =
            new Dictionary<string, DescribeCriterionFor>();

        public IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<ApplicabilityCriterionDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }

        public DescribeCriterionFor For(string category) {
            return For(category, null, null);
        }

        public DescribeCriterionFor For(
            string category, LocalizedString name, LocalizedString description) {
            DescribeCriterionFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeCriterionFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }
}
