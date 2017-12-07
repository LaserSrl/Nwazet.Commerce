using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentTypes.Events;
using Orchard.UI.Notify;
using Orchard.ContentManagement.MetaData.Models;
using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Settings {
    [OrchardFeature("Territories.Localization")]
    public class TerritoryLocalizationEditorEvents 
        : IContentDefinitionEventHandler {
        // Call HierarchyType a ContentType that has a TerritoryHierarchyPart
        // Call TerritoryType a ContentType that has a TerritoryPart
        // If there are any HierarchyTypes that have a LocalizationPart, then all TerritoryTypes must have a LocalizationPart.
        // This class enforces this situation.

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INotifier _notifier;

        public TerritoryLocalizationEditorEvents(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier) {

            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }
        
        private Localizer T;

        public void ContentPartAttached(ContentPartAttachedContext context) {
            // If we attached a LocalizationPart to a HierarchyType, and there are no other
            // HierarchyTypes that have it, update all TerritoryTypes. (if there already are some
            // we should have done that update already)
            if ( // Added a LocalizationPart
                context.ContentPartName.Equals("LocalizationPart", StringComparison.InvariantCultureIgnoreCase)
                && // to a HierarchyType
                HierarchyTypes.Any(ctd => ctd.Name.Equals(context.ContentTypeName)) 
                && // and no other HierarchyType had a LocalizationPart already
                !HierarchyTypes
                    .Where(ctd => !ctd.Name.Equals(context.ContentTypeName))
                    .Any(TypeHasLocalizationPart)) {
                // Add LocalizationPart to all TerritoryTypes
                foreach (var territoryType in TerritoryTypes) {
                    AddLocalizationToType(territoryType);
                }
            }
        }
        
        public void ContentPartDetached(ContentPartDetachedContext context) {
            // If we detached a LocalizationPart from a TerritoryType, but there are HierarchyTypes
            // that have it, attach it back
            if (// We detached a LocalizationPart
                context.ContentPartName.Equals("LocalizationPart", StringComparison.InvariantCultureIgnoreCase)
                && // from a TerritoryType
                TerritoryTypes.Any(ctd => ctd.Name.Equals(context.ContentTypeName))
                && // but there are HierarchyTypes with a LocalizationPart
                HierarchyTypes
                    .Any(TypeHasLocalizationPart)) {
                // Add the LocalizationPart back to the type
                AddLocalizationToType(context.ContentTypeName);
            }
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context) {
            // If we created a HierarchyType or a TerritoryType, there may be something that we have to do.
            HandleNewType(context.ContentTypeDefinition);
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            // If we imported a HierarchyType or a TerritoryType, there may be something that we have to do.
            HandleNewType(context.ContentTypeDefinition);
        }
        
        private void HandleNewType(ContentTypeDefinition definition) {

            if (// If it's a HierarchyType
                TypeHasPart(definition, TerritoryHierarchyPart.PartName)
                && // that has a LocalizationPart
                TypeHasLocalizationPart(definition)
                && // and no other HierarchyType had a LocalizationPart already
                !HierarchyTypes
                    .Where(ctd => !ctd.Name.Equals(definition.Name))
                    .Any(TypeHasLocalizationPart)) {
                // Add LocalizationPart to all TerritoryTypes
                foreach (var territoryType in TerritoryTypes) {
                    AddLocalizationToType(territoryType);
                }
            }
            if (// If it's a TerritoryType
                TypeHasPart(definition, TerritoryPart.PartName)
                && // that does not have a LocalizationPart already
                !TypeHasLocalizationPart(definition)
                && // but there are HierarchyTypes with a LocalizationPart
                HierarchyTypes
                    .Any(TypeHasLocalizationPart)) {
                // Add the LocalizationPart back to the type
                AddLocalizationToType(definition);
            }
        }

        private void AddLocalizationToType(string typeName) {
            _contentDefinitionManager
                .AlterTypeDefinition(typeName, builder => {
                    builder.WithPart("LocalizationPart");
                });
            var name = _contentDefinitionManager.GetTypeDefinition(typeName).DisplayName;
            name = string.IsNullOrWhiteSpace(name) ? typeName : name;
            _notifier.Information(T("Contents of type \"{0}\" must have a LocalizationPart, so it was added.", name));
        }

        private void AddLocalizationToType(ContentTypeDefinition type) {
            _contentDefinitionManager
                .AlterTypeDefinition(type.Name, builder => {
                    builder.WithPart("LocalizationPart");
                });
            var name = type.DisplayName;
            name = string.IsNullOrWhiteSpace(name) ? type.Name : name;
            _notifier.Information(T("Contents of type \"{0}\" must have a LocalizationPart, so it was added.", name));
        }

        private bool TypeHasLocalizationPart(ContentTypeDefinition type) {
            return TypeHasPart(type, "LocalizationPart");
        }

        private bool TypeHasPart(ContentTypeDefinition type, string partName) {
            return type
                .Parts
                .Any(ctpd => ctpd
                    .PartDefinition
                    .Name
                    .Equals(partName, StringComparison.InvariantCultureIgnoreCase));
        }

        private IEnumerable<ContentTypeDefinition> HierarchyTypes {
            get { return _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(ctd => TypeHasPart(ctd, TerritoryHierarchyPart.PartName));
            }
        }

        private IEnumerable<ContentTypeDefinition> TerritoryTypes {
            get {
                return _contentDefinitionManager
                  .ListTypeDefinitions()
                  .Where(ctd => TypeHasPart(ctd, TerritoryPart.PartName));
            }
        }

        #region Not implemented IContentDefinitionEventHandler methods
        public void ContentFieldAttached(ContentFieldAttachedContext context) { }

        public void ContentFieldDetached(ContentFieldDetachedContext context) { }

        public void ContentPartCreated(ContentPartCreatedContext context) { }

        public void ContentTypeImporting(ContentTypeImportingContext context) { }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) { }

        public void ContentPartRemoved(ContentPartRemovedContext context) { }

        public void ContentPartImporting(ContentPartImportingContext context) { }

        public void ContentPartImported(ContentPartImportedContext context) { }
        #endregion
    }
}
