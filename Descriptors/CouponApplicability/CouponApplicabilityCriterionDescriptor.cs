using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Descriptors.CouponApplicability {
    public class CouponApplicabilityCriterionDescriptor {

        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public Action<CouponCriterionContext> AdditionCriterion { get; set; }
        public Action<CouponCriterionContext> ProcessingCriterion { get; set; }
        public string Form { get; set; }
        public Func<CouponCriterionContext, LocalizedString> Display { get; set; }
    }
}
