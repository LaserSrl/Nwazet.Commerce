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
    public class CouponCriterionDescriptor {

        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public string Form { get; set; }


        public bool IsAvailableForConfiguration { get; set; }
        public bool IsAvailableForProcessing { get; set; }

        public Func<CouponContext, LocalizedString> Display { get; set; }
    }
    
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicabilityCriterionDescriptor
        : CouponCriterionDescriptor {

        public Action<CouponApplicabilityCriterionContext> AdditionCriterion { get; set; }
        public Action<CouponApplicabilityCriterionContext> ProcessingCriterion { get; set; }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponLineApplicabilityCriterionDescriptor
        : CouponCriterionDescriptor {

        public Action<CouponLineCriterionContext> Criterion { get; set; }
        /// <summary>
        /// Message for the case where the criterion fails for all
        /// cart lines.
        /// </summary>
        public Func<CouponApplicabilityContext, LocalizedString> FailureMessage { get; set; }
    }
}
