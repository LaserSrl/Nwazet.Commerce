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
            IContentManager contentManager,
            ITerritoriesPermissionProvider permissionProvider
            ) {

            _orchardServices = orchardServices;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }

        public IEnumerable<ContentTypeDefinition> GetTerritoryTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)) &&
                        _orchardServices.Authorizer.Authorize(TerritoriesPermissions.GetTerritoryPermission(ctd)));
        }
        
        public IEnumerable<ContentTypeDefinition> GetHierarchyTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryHierarchyPart.PartName, StringComparison.InvariantCultureIgnoreCase)) &&
                         _orchardServices.Authorizer.Authorize(TerritoriesPermissions.GetHierarchyPermission(ctd)));
        }
        
        public IContentQuery<TerritoryHierarchyPart, TerritoryHierarchyPartRecord> GetHierarchiesQuery() {
            return GetHierarchiesQuery(VersionOptions.Latest);
        }

        public IContentQuery<TerritoryHierarchyPart> GetHierarchiesQuery(params string[] contentTypes) {
            return GetHierarchiesQuery(VersionOptions.Latest, contentTypes);
        }

        public IContentQuery<TerritoryHierarchyPart, TerritoryHierarchyPartRecord> GetHierarchiesQuery(VersionOptions versionOptions) {
            return _contentManager
                .Query<TerritoryHierarchyPart, TerritoryHierarchyPartRecord>()
                .ForVersion(versionOptions);
        }

        public IContentQuery<TerritoryHierarchyPart> GetHierarchiesQuery(
            VersionOptions versionOptions, params string[] contentTypes) {
            if (contentTypes != null && contentTypes.Any()) {
                return GetHierarchiesQuery(versionOptions).ForType(contentTypes);
            }
            return GetHierarchiesQuery(versionOptions);
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
