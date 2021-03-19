using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    public class CouponCriteriaAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>> Criteria { get; set; }
    }
}
