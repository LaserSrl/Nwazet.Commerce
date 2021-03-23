using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {
    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeCouponApplicabilityContext {

        private readonly Dictionary<string, DescribeCouponApplicabilityFor> _describes =
            new Dictionary<string, DescribeCouponApplicabilityFor>();

        public IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<CouponApplicabilityCriterionDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }


        public DescribeCouponApplicabilityFor For(string category) {
            return For(category, null, null);
        }

        public DescribeCouponApplicabilityFor For(
            string category, LocalizedString name, LocalizedString description) {
            DescribeCouponApplicabilityFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeCouponApplicabilityFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }
    
    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeCouponLineApplicabilityContext {

        private readonly Dictionary<string, DescribeCouponLineApplicabilityFor> _describes =
            new Dictionary<string, DescribeCouponLineApplicabilityFor>();

        public IEnumerable<TypeDescriptor<CouponLineApplicabilityCriterionDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<CouponLineApplicabilityCriterionDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }


        public DescribeCouponLineApplicabilityFor For(string category) {
            return For(category, null, null);
        }

        public DescribeCouponLineApplicabilityFor For(
            string category, LocalizedString name, LocalizedString description) {
            DescribeCouponLineApplicabilityFor describeFor;
            if (!_describes.TryGetValue(category, out describeFor)) {
                describeFor = new DescribeCouponLineApplicabilityFor(category, name, description);
                _describes[category] = describeFor;
            }
            return describeFor;
        }
    }
}
