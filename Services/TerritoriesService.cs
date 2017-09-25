using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Territories")]
    public class TerritoriesService : ITerritoriesService {
        private readonly IAuthorizer _authorizer;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TerritoriesService(
            IAuthorizer authorizer,
            IContentDefinitionManager contentDefinitionManager
            ) {

            _authorizer = authorizer;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IEnumerable<ContentTypeDefinition> GetTerritoryTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)) &&
                    _authorizer.Authorize(GetTerritoryPermission(ctd)));
        }

        public IEnumerable<Permission> ListTerritoryTypePermissions() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)))
                .Select(ctd => GetTerritoryPermission(ctd));
        }

        public Permission GetTerritoryPermission(ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = string.Format(TerritoriesPermissions.ManageTerritory.Name, typeDefinition.Name),
                Description = string.Format(TerritoriesPermissions.ManageTerritory.Description, typeDefinition.Name),
                ImpliedBy = TerritoriesPermissions.ManageTerritory.ImpliedBy
            };
        }
        
        public IEnumerable<ContentTypeDefinition> GetHierarchyTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryHierarchyPart.PartName, StringComparison.InvariantCultureIgnoreCase)) && 
                    _authorizer.Authorize(GetHierarchyPermission(ctd)));
        }

        public IEnumerable<Permission> ListHierarchyTypePermissions() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryHierarchyPart.PartName, StringComparison.InvariantCultureIgnoreCase)))
                .Select(ctd => GetHierarchyPermission(ctd));
        }

        public Permission GetHierarchyPermission(ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = string.Format(TerritoriesPermissions.ManageTerritoryHierarchy.Name, typeDefinition.Name),
                Description = string.Format(TerritoriesPermissions.ManageTerritoryHierarchy.Description, typeDefinition.Name),
                ImpliedBy = TerritoriesPermissions.ManageTerritoryHierarchy.ImpliedBy
            };
        }
    }
}
