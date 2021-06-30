using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponCriteriaManagementService : IDependency {
        IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>> DescribeApplicabilityCriteria();
        CouponApplicabilityCriterionDescriptor GetCriterion(string category, string type);
        void DeleteCriterion(int criterionId);

        IEnumerable<TypeDescriptor<CouponLineApplicabilityCriterionDescriptor>> DescribeLineCriteria();
        CouponLineApplicabilityCriterionDescriptor GetLineCriterion(string category, string type);
        void DeleteLineCriterion(int criterionId);
    }
}
