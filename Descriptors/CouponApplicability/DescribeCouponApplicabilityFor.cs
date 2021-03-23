using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {
    [OrchardFeature("Nwazet.Couponing")]
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
            Action<CouponApplicabilityCriterionContext> additionCriterion,
            Action<CouponApplicabilityCriterionContext> processingCriterion,
            Func<CouponApplicabilityCriterionContext, LocalizedString> display,
            string form = null) {

            Types.Add(new CouponApplicabilityCriterionDescriptor {
                Type = type,
                Name = name,
                Description = description,
                Category = _category,
                AdditionCriterion = additionCriterion,
                ProcessingCriterion = processingCriterion,
                Display = display,
                Form = form
            });
            return this;
        }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeCouponLineApplicabilityFor {
        private readonly string _category;

        public DescribeCouponLineApplicabilityFor(
            string category, LocalizedString name, LocalizedString description) {
            Types = new List<CouponLineApplicabilityCriterionDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<CouponLineApplicabilityCriterionDescriptor> Types { get; private set; }

        public DescribeCouponLineApplicabilityFor Element(
            string type,
            LocalizedString name,
            LocalizedString description,
            Action<CouponLineCriterionContext> criterion,
            Func<CouponLineCriterionContext, LocalizedString> display,
            string form = null) {

            Types.Add(new CouponLineApplicabilityCriterionDescriptor {
                Type = type,
                Name = name,
                Description = description,
                Category = _category,
                Criterion = criterion,
                Display = display,
                Form = form
            });
            return this;
        }
    }
}
