using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;

namespace Nwazet.Commerce.Descriptors.ApplicabilityCriterion {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class DescribeCriterionFor {
        private readonly string _category;

        public DescribeCriterionFor(string category, LocalizedString name, LocalizedString description) {
            Types = new List<ApplicabilityCriterionDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<ApplicabilityCriterionDescriptor> Types { get; private set; }

        public DescribeCriterionFor Element(
            string type,
            LocalizedString name,
            LocalizedString description,
            Action<CriterionContext> criterion,
            Func<CriterionContext, LocalizedString> display,
            string form = null) {

            Types.Add(new ApplicabilityCriterionDescriptor {
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
