using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Nwazet.Commerce.Filters {
    public class UserRolesFilterForm : IFormProvider {

        public const string FormName = "UserRolesFilterForm";
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        private readonly IRoleService _roleService;

        public UserRolesFilterForm(
            IShapeFactory shapeFactory,
            IRoleService roleService) {
            Shape = shapeFactory;

            _roleService = roleService;

            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "UserRolesFilterForm",
                        _roles: Shape.SelectList(
                            Id: "roles", Name: "Roles",
                            Title: T("Roles"),
                            Size: 1,
                            Multiple: true
                        ),
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ));

                    var roles = _roleService.GetRoles().OrderBy(record => record.Name);
                    foreach (var role in roles) {
                        f._roles.Add(new SelectListItem {
                            Value = role.Name,
                            Text = role.Name
                        });
                    }

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(UserRolesOperator.MustHaveOne),
                        Text = T("The user must have at least one of the selected roles").Text
                    });

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(UserRolesOperator.MustHaveAll),
                        Text = T("The user must have all the selected roles").Text
                    });

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(UserRolesOperator.MustHaveNoOne),
                        Text = T("The user must have no one of the selected roles").Text
                    });
                    return f;
                };
            context.Form(FormName, form);
        }

        public static LocalizedString DisplayFilter(Localizer T, dynamic state) {
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
    }
}

public enum UserRolesOperator {
   MustHaveOne,
   MustHaveAll,
   MustHaveNoOne
}
