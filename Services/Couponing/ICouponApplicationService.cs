using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponApplicationService : IDependency {
        /// <summary>
        /// Attemtps to add a coupon with a given code to the shopping cart
        /// </summary>
        /// <param name="code"></param>
        void ApplyCoupon(string code);
        /// <summary>
        /// Attempts to remove a coupon with the given code from the shopping cart
        /// </summary>
        /// <param name="code"></param>
        void RemoveCoupon(string code);

        void ReevaluateValidity(CouponLifeUpdateContext context);
        void CouponUsed(CouponUsedContext context);

        bool CanProcess(CouponApplicabilityContext context);

        IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>> DescribeApplicabilityCriteria();
        CouponApplicabilityCriterionDescriptor GetCriterion(string category, string type);
        void DeleteCriterion(int criterionId);
    }
}
