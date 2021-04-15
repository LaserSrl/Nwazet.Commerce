using Nwazet.Commerce.Descriptors.CouponApplicability;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponCriterionEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public CouponApplicabilityCriterionDescriptor Criterion { get; set; }
        public dynamic Form { get; set; }
    }
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponLineCriterionEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public CouponLineApplicabilityCriterionDescriptor Criterion { get; set; }
        public dynamic Form { get; set; }
    }
}
