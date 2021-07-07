using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Security;
using System;
using System.Linq;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class UserRolesCouponApplicabilityCriterion
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;

        public UserRolesCouponApplicabilityCriterion(
           IWorkContextAccessor workContextAccessor,
           ICacheManager cacheManager,
           ISignals signals,
           IRepository<UserRolesPartRecord> userRolesRepository)
           : base(workContextAccessor, cacheManager, signals) {
            _userRolesRepository = userRolesRepository;
        }

        public override string ProviderName =>
            "UserRolesCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName =>
            T("Coupon applicability criterion according to user associated role.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("UserRoles", T("User roles"), T("User roles"))
                .Element("Roles for user",
                    T("Roles for user"),
                    T("Roles for user"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => DisplayFilter(ctx.State),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    UserRolesFilterForm.FormName);
        }


       private LocalizedString DisplayFilter(dynamic state) {
            var roles = state.Roles != null ? (string)state.Roles : string.Empty;
            var op = (UserRolesOperator)Enum.Parse(typeof(UserRolesOperator), Convert.ToString(state.Operator));

            switch (op) {
                case UserRolesOperator.MustHaveOne:
                return T("The user must have at least one of these roles: {0}", roles);
                case UserRolesOperator.MustHaveAll:
                return T("The user must have all these roles: {0}", roles);
                case UserRolesOperator.MustHaveNoOne:
                return T("The user must have no one of these roles: {0}", roles);
                default:
                throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyCriterion(
            CouponApplicabilityCriterionContext context) {

            if (context.IsApplicable) {
                if(context?.ApplicabilityContext?.WorkContext?.CurrentUser != null) {
                    var user = context.ApplicabilityContext.WorkContext.CurrentUser;

                    // do configured test
                    var result = EvaluateFilter(user, context.State);

                    context.IsApplicable = result;
                    context.ApplicabilityContext.IsApplicable = result;
                }
                else {
                    context.IsApplicable = false;
                    context.ApplicabilityContext.IsApplicable = false;
                }
            }
        }

        private bool EvaluateFilter(IUser user, dynamic state) {
            var roles = state.Roles != null ? (string)state.Roles : string.Empty;

            var rolesList = roles.Split(',').ToList();

            var op = (UserRolesOperator)Enum.Parse(typeof(UserRolesOperator), Convert.ToString(state.Operator));

            var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == user.Id).ToArray();
            var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role.Name);

            switch (op) {
                case UserRolesOperator.MustHaveOne:
                    return rolesList.Any(r => currentRoleRecords.Contains(r));
                case UserRolesOperator.MustHaveAll:
                    return rolesList.All(r => currentRoleRecords.Contains(r));
                case UserRolesOperator.MustHaveNoOne:
                    return !rolesList.Any(r => currentRoleRecords.Contains(r));
                default:
                    return false;
            }
        }
    }
}
