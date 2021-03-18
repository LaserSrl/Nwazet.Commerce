using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {
    public class DescribeCouponApplicabilityFor {
        private readonly string _category;

        public DescribeCouponApplicabilityFor(
            string category, LocalizedString name, LocalizedString description) {
            Types = new List<CouponApplicabilityCriterionDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<CouponApplicabilityCriterionDescriptor> Types { get; private set; }

        public DescribeCouponApplicabilityFor Element(
            string type,
            LocalizedString name,
            LocalizedString description,
            Action<CouponCriterionContext> criterion,
            Func<CouponCriterionContext, LocalizedString> display,
            string form = null) {

            Types.Add(new CouponApplicabilityCriterionDescriptor {
                Type = type,
                Name = name,
                Description = description,
                Category = _category,
                TestCriterion = criterion,
                Display = display,
                Form = form
            });
            return this;
        }
    }
}
