﻿using Nwazet.Commerce.Models;
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
        
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;

        public TerritoriesService(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizer authorizer
            ) {
            
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizer = authorizer;
        }

        public IEnumerable<ContentTypeDefinition> GetTerritoryTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)) &&
                        _authorizer.Authorize(TerritoriesPermissions.GetTerritoryPermission(ctd)));
        }
        
        public IEnumerable<ContentTypeDefinition> GetHierarchyTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(TerritoryHierarchyPart.PartName, StringComparison.InvariantCultureIgnoreCase)) &&
                         _authorizer.Authorize(TerritoriesPermissions.GetHierarchyPermission(ctd)));
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
            return GetTerritoriesQuery(hierarchyPart, versionOptions: null);
        }

        public IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(TerritoryHierarchyPart hierarchyPart, VersionOptions versionOptions) {
            if (hierarchyPart == null || hierarchyPart.Record == null) {
                throw new ArgumentNullException("hierarchyPart");
            }

            versionOptions = versionOptions ??
                (hierarchyPart.ContentItem.IsPublished() ? VersionOptions.Published : VersionOptions.Latest);
            
            return _contentManager
                .Query<TerritoryPart, TerritoryPartRecord>()
                .WithQueryHints(new QueryHints().ExpandRecords("TerritoryHierarchyPartRecord"))
                .ForVersion(versionOptions)
                .Where(tpr => tpr.Hierarchy.Id == hierarchyPart.Record.Id);
        }

        public IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(
            TerritoryHierarchyPart hierarchyPart, TerritoryPart territoryPart) {

            return GetTerritoriesQuery(hierarchyPart, territoryPart, null);
        }

        public IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(
            TerritoryHierarchyPart hierarchyPart, TerritoryPart territoryPart, VersionOptions versionOptions) {
            
            var baseQuery = GetTerritoriesQuery(hierarchyPart, versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords("TerritoryPartRecord"));

            if (territoryPart == null) {
                return baseQuery
                    .Where(tpr => tpr.ParentTerritory == null);
            } else {
                return baseQuery
                    .Where(tpr => tpr.ParentTerritory.Id == territoryPart.Record.Id);
            }
        }
    }
}
