using Orchard.Environment.Extensions;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;
using Nwazet.Commerce.Services;

namespace Nwazet.Commerce.Permissions {
    [OrchardFeature("Territories")]
    public class TerritoriesPermissions : IPermissionProvider {

        #region Base Permissions definitions
        public static readonly Permission ManageTerritories = new Permission {
            Description = "Manage Territories",
            Name = "ManageTerritories"
        };

        public static readonly Permission ManageTerritory = new Permission {
            Description = "Manage Territories of type {0}",
            Name = "ManageTerritories_{0}",
            ImpliedBy = new[] { ManageTerritories }
        };

        public static readonly Permission ManageTerritoryHierarchies = new Permission {
            Description = "Manage hierarchies of territories",
            Name = "ManageTerritoryHierarchies"
        };

        public static readonly Permission ManageTerritoryHierarchy = new Permission {
            Description = "Manage hierarchies of territories of type {0}",
            Name = "ManageTerritoryHierarchies_{0}",
            ImpliedBy = new[] { ManageTerritoryHierarchies }
        };

        public static readonly Permission ManageInternalTerritories = new Permission {
            Description = "Manage the Territories' internal structure",
            Name = "ManageInternalTerritories"
        };
        #endregion

        private readonly ITerritoriesService _territoriesService;

        public TerritoriesPermissions(
            ITerritoriesService territoriesService) {

            _territoriesService = territoriesService;
        }

        public Feature Feature { get; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();
            //Base permissions
            permissions.Add(ManageTerritories);
            permissions.Add(ManageTerritoryHierarchies);
            //Dynamic permissions are defined per type of hierarchy (not per single hierarchy, as
            //is the case for example in menus)
            permissions.AddRange(_territoriesService.ListHierarchyTypePermissions());
            permissions.AddRange(_territoriesService.ListTerritoryTypePermissions());

            return permissions;
        }


    }
}
