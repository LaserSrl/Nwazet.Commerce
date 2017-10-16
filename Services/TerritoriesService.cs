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
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Territories")]
    public class TerritoriesService : ITerritoriesService {
        
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public TerritoriesService(
            IOrchardServices orchardServices,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager
            ) {

            _orchardServices = orchardServices;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }

        public IEnumerable<ContentTypeDefinition> GetTerritoryTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)) &&
                        _orchardServices.Authorizer.Authorize(GetTerritoryPermission(ctd)));
        }

        public IEnumerable<Permission> ListTerritoryTypePermissions() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)))
                .Select(ctd => GetTerritoryPermission(ctd));
        }

        private Permission GetTerritoryPermission(ContentTypeDefinition typeDefinition) {
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
                         _orchardServices.Authorizer.Authorize(GetHierarchyPermission(ctd)));
        }

        public IEnumerable<Permission> ListHierarchyTypePermissions() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryHierarchyPart.PartName, StringComparison.InvariantCultureIgnoreCase)))
                .Select(ctd => GetHierarchyPermission(ctd));
        }

        private Permission GetHierarchyPermission(ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = string.Format(TerritoriesPermissions.ManageTerritoryHierarchy.Name, typeDefinition.Name),
                Description = string.Format(TerritoriesPermissions.ManageTerritoryHierarchy.Description, typeDefinition.Name),
                ImpliedBy = TerritoriesPermissions.ManageTerritoryHierarchy.ImpliedBy
            };
        }

        public IContentQuery<TerritoryHierarchyPart, TerritoryHierarchyPartRecord> GetHierarchiesQuery() {
            return GetHierarchiesQuery(VersionOptions.Latest);
        }

        public IContentQuery<TerritoryHierarchyPart, TerritoryHierarchyPartRecord> GetHierarchiesQuery(VersionOptions versionOptions) {
            return _contentManager
                .Query<TerritoryHierarchyPart, TerritoryHierarchyPartRecord>()
                .ForVersion(versionOptions);
        }

        public IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(TerritoryHierarchyPart hierarchyPart) {
            return GetTerritoriesQuery(hierarchyPart, VersionOptions.Latest);
        }

        public IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(TerritoryHierarchyPart hierarchyPart, VersionOptions versionOptions) {
            return _contentManager
                .Query<TerritoryPart, TerritoryPartRecord>()
                .ForVersion(versionOptions);
        }

    }
}
