using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Permissions {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingPermissions : IPermissionProvider {
        public static readonly Permission ManageCoupons = new Permission {
            Description = "Manage coupons",
            Name = "ManageCoupons"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageCoupons
            };
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageCoupons }
                },
            };
        }
    }
}
