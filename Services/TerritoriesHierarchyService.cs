using Nwazet.Commerce.Exceptions;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Territories")]
    public class TerritoriesHierarchyService : ITerritoriesHierarchyService {

        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TerritoriesHierarchyService(
            ITerritoriesRepositoryService territoriesRepositoryService,
            IContentDefinitionManager contentDefinitionManager) {

            _territoriesRepositoryService = territoriesRepositoryService;
            _contentDefinitionManager = contentDefinitionManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        public void AddTerritory(TerritoryPart territory, TerritoryHierarchyPart hierarchy) {
            if (territory == null || territory.Record == null) {
                throw new ArgumentNullException("territory");
            }
            if (hierarchy == null || hierarchy.Record == null) {
                throw new ArgumentNullException("hierarchy");
            }
            // check that types are correct
            if (territory.ContentItem.ContentType != hierarchy.TerritoryType) {
                var territoryTypeText = territory.ContentItem
                    .TypeDefinition.DisplayName;
                var hierarchyTerritoryTypeText = _contentDefinitionManager
                    .GetTypeDefinition(hierarchy.TerritoryType).DisplayName;
                throw new ArrayTypeMismatchException(
                    T("The ContentType for the Territory ({0}) does not match the expected TerritoryType for the hierarchy ({1})",
                        territoryTypeText, hierarchyTerritoryTypeText).Text);
            }
            // set hierarchy
            territory.Record.Hierarchy = hierarchy.Record;
            // remove parent: This method always puts the territory at the root level of the hierarchy
            territory.Record.ParentTerritory = null;
            // set the hierarchy for all children
            foreach (var childRecord in territory.Record.Children) {
                childRecord.Hierarchy = hierarchy.Record;
            }
        }

        public void AddTerritory(TerritoryPart territory, TerritoryHierarchyPart hierarchy, TerritoryPart parent) {
            if (parent == null || parent.Record == null) {
                throw new ArgumentNullException("parent");
            }
            if (parent.Record.Hierarchy == null) {
                throw new ArgumentNullException("parent", T("The hierarchy for the Territory must not be null.").Text);
            }
            if (parent.Record.Hierarchy.Id != hierarchy.Record.Id) {
                throw new ArrayTypeMismatchException(T("The two territories must belong to the same hierarchy.").Text);
            }
            AddTerritory(territory, hierarchy);
            // finally move
            territory.Record.ParentTerritory = parent.Record;
        }

        public void AssignParent(TerritoryPart territory, TerritoryPart parent) {
            if (territory == null || territory.Record == null) {
                throw new ArgumentNullException("territory");
            }
            if (parent == null || parent.Record == null) {
                throw new ArgumentNullException("parent");
            }
            // verify type
            if (territory.ContentItem.ContentType != parent.ContentItem.ContentType) {
                var territoryTypeText = territory.ContentItem
                    .TypeDefinition.DisplayName;
                var parentTypeText = parent.ContentItem
                    .TypeDefinition.DisplayName;
                throw new ArrayTypeMismatchException(
                    T("The ContentType for the Territory ({0}) does not match the ContentType for the parent ({1})",
                        territoryTypeText, parentTypeText).Text);
            }
            // verify hierarchies.
            if (territory.Record.Hierarchy == null) {
                throw new ArgumentNullException("territory", T("The hierarchy for the Territory must not be null.").Text);
            }
            if (parent.Record.Hierarchy == null) {
                throw new ArgumentNullException("parent", T("The hierarchy for the Territory must not be null.").Text);
            }
            if (parent.Record.Hierarchy.Id != territory.Record.Hierarchy.Id) {
                throw new ArrayTypeMismatchException(T("The two territories must belong to the same hierarchy.").Text);
            }
            // finally move
            territory.Record.ParentTerritory = parent.Record;
        }

        public void AssignInternalRecord(TerritoryPart territory, string name) {
            var internalRecord = _territoriesRepositoryService.GetTerritoryInternal(name);
            if (internalRecord == null) {
                throw new ArgumentNullException("name", T("No TerritoryInternalRecord exists with the name provided (\"{0}\")", name).Text);
            }
            AssignInternalRecord(territory, internalRecord);
        }

        public void AssignInternalRecord(TerritoryPart territory, int id) {
            var internalRecord = _territoriesRepositoryService.GetTerritoryInternal(id);
            if (internalRecord == null) {
                throw new ArgumentNullException("id", T("No TerritoryInternalRecord exists with the id provided (\"{0}\")", id).Text);
            }
            AssignInternalRecord(territory, internalRecord);
        }

        public void AssignInternalRecord(TerritoryPart territory, TerritoryInternalRecord internalRecord) {
            if (territory == null || territory.Record == null) {
                throw new ArgumentNullException("territory");
            }
            if (internalRecord == null) {
                throw new ArgumentNullException("internalRecord");
            }
            // check that the internal record does not exist yet in the same hierarchy
            var hierarchyRecord = territory.Record.Hierarchy;
            if (hierarchyRecord != null) {
                if (hierarchyRecord
                        .Territories
                        .Where(tpr => tpr.Id != territory.Record.Id) // exclude current territory
                        .Select(tpr => tpr.TerritoryInternalRecord)
                        .Any(tir => tir.Id == internalRecord.Id)) {

                    throw new TerritoryInternalDuplicateException(T("The selected territory is already assigned in the current hierarchy."));
                }
            }
            territory.Record.TerritoryInternalRecord = internalRecord;
        }


    }
}
