using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {

    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeApplicabilityContext<TFor, TDescriptor>
        where TFor : DescribeApplicabilityFor<TDescriptor>
        where TDescriptor : CouponCriterionDescriptor {

        protected readonly Dictionary<string, TFor> _describes =
            new Dictionary<string, TFor>();

        public IEnumerable<TypeDescriptor<TDescriptor>> Describe() {
            return _describes.Select(kp => new TypeDescriptor<TDescriptor> {
                Category = kp.Key,
                Name = kp.Value.Name,
                Description = kp.Value.Description,
                Descriptors = kp.Value.Types
            });
        }
        
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeCouponApplicabilityContext
        : DescribeApplicabilityContext<DescribeCouponApplicabilityFor, CouponApplicabilityCriterionDescriptor> {
        
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
    public class DescribeCouponLineApplicabilityContext
        : DescribeApplicabilityContext<DescribeCouponLineApplicabilityFor, CouponLineApplicabilityCriterionDescriptor> {
        
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
