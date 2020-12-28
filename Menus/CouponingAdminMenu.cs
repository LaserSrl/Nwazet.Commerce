using Nwazet.Commerce.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Menus {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingAdminMenu : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public CouponingAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("nwazet-commerce")
                .Add(item => item
                    .Caption(T("Commerce"))
                    .Position("2")
                    .LinkToFirstChild(false)

                    .Add(subItem => subItem
                        .Caption(T("Coupons"))
                        .Position("2.5")
                        .Action("Index", "CouponingAdmin", new { area = "Nwazet.Commerce" })
                        .Permission(CouponingPermissions.ManageCoupons)
                    )

                );
        }
    }
}
