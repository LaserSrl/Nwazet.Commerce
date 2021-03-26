using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {

    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeApplicabilityFor<TDescriptor>
        where TDescriptor : CouponCriterionDescriptor {

        protected readonly string _category;

        public DescribeApplicabilityFor(
            string category, LocalizedString name, LocalizedString description) {
            Types = new List<TDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; protected set; }
        public LocalizedString Description { get; protected set; }
        public List<TDescriptor> Types { get; protected set; }

    }

    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeCouponApplicabilityFor 
        : DescribeApplicabilityFor<CouponApplicabilityCriterionDescriptor> {

        public DescribeCouponApplicabilityFor(
            string category, LocalizedString name, LocalizedString description)
            : base(category, name, description) {
        }

        public DescribeCouponApplicabilityFor Element(
            string type,
            LocalizedString name,
            LocalizedString description,
            Action<CouponApplicabilityCriterionContext> additionCriterion,
            Action<CouponApplicabilityCriterionContext> processingCriterion,
            Func<CouponContext, LocalizedString> display,
            bool forConfiguration, bool forProcessing,
            string form = null) {

            if (Types == null) {
                Types = new List<CouponApplicabilityCriterionDescriptor>();
            }

            Types.Add(new CouponApplicabilityCriterionDescriptor {
                Type = type,
                Name = name,
                Description = description,
                Category = _category,
                AdditionCriterion = additionCriterion,
                ProcessingCriterion = processingCriterion,
                Display = display,
                IsAvailableForConfiguration = forConfiguration,
                IsAvailableForProcessing = forProcessing,
                Form = form
            });
            return this;
        }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class DescribeCouponLineApplicabilityFor
        : DescribeApplicabilityFor<CouponLineApplicabilityCriterionDescriptor> {

        public DescribeCouponLineApplicabilityFor(
            string category, LocalizedString name, LocalizedString description)
            : base(category, name, description) {
        }

        public DescribeCouponLineApplicabilityFor Element(
            string type,
            LocalizedString name,
            LocalizedString description,
            Action<CouponLineCriterionContext> criterion,
            Func<CouponContext, LocalizedString> display,
            Func<CouponApplicabilityContext, LocalizedString> failureMessage,
            bool forConfiguration, bool forProcessing,
            string form = null) {

            if (Types == null) {
                Types = new List<CouponLineApplicabilityCriterionDescriptor>();
            }

            Types.Add(new CouponLineApplicabilityCriterionDescriptor {
                Type = type,
                Name = name,
                Description = description,
                Category = _category,
                Criterion = criterion,
                Display = display,
                FailureMessage = failureMessage,
                IsAvailableForConfiguration = forConfiguration,
                IsAvailableForProcessing = forProcessing,
                Form = form
            });
            return this;
        }
    }
}
