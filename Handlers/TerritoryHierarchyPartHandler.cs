using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;
        private readonly ITerritoryPartRecordService _territoryPartRecordService;

        public TerritoryHierarchyPartHandler(
            IRepository<TerritoryHierarchyPartRecord> repository,
            IContentManager contentManager,
            ITerritoryPartRecordService territoryPartRecordService) {

            _contentManager = contentManager;
            _territoryPartRecordService = territoryPartRecordService;

            Filters.Add(StorageFilter.For(repository));

            // TerritoryHierarchyPart.TerritoryType must be populated
            OnInitializing<TerritoryHierarchyPart>((ctx, part) => 
                part.TerritoryType = part.Settings.GetModel<TerritoryHierarchyPartSettings>().TerritoryType);
            OnLoaded<TerritoryHierarchyPart>((ctx, part) =>
                part.TerritoryType = string.IsNullOrWhiteSpace(part.TerritoryType) ? 
                    part.Settings.GetModel<TerritoryHierarchyPartSettings>().TerritoryType : 
                    part.TerritoryType);

            //Lazyfield setters
            OnInitializing<TerritoryHierarchyPart>(PropertySetHandlers);
            //OnInitializing<TerritoryHierarchyPart>(LazyLoadHandlers);
            OnLoading<TerritoryHierarchyPart>((ctx, part) => LazyLoadHandlers(part));
            OnVersioning<TerritoryHierarchyPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

            //Handle the presence of territories in a hierarchy: may need to run asynchronously
            OnRemoving<TerritoryHierarchyPart>(RemoveTerritoriesInHierarchy);
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var hierarchy = context.ContentItem.As<TerritoryHierarchyPart>();

            if (hierarchy == null)
                return;

            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", TerritoriesUtilities.Area},
                {"Controller", TerritoriesUtilities.HierarchyEditController},
                {"Action", TerritoriesUtilities.HierarchyEditAction},
                {"Id", hierarchy.Id}
            };

        }

        static void PropertySetHandlers(
            InitializingContentContext context, TerritoryHierarchyPart part) {
            
            part.TopLevelField.Setter(value => {
                var actualItems = value.Where(ci => ci.As<TerritoryPart>() != null);
                //// commented because now the territories are extracted on the spot from the database
                //part.Record.Territories = actualItems.Any() ?
                //    actualItems.Select(ci => ci.As<TerritoryPart>().Record).ToList() :
                //    new List<TerritoryPartRecord>();
                return actualItems;
            });

            //call the setter in case a value had already been set
            if (part.TopLevelField.Value != null) {
                part.TopLevelField.Value = part.TopLevelField.Value;
            }
        }

        void LazyLoadHandlers(TerritoryHierarchyPart part) {
            
            part.TopLevelField.Loader(() => {
                //if (part.Record.Territories != null && part.Record.Territories.Any()) {
                if (_territoryPartRecordService.GetHierarchyTerritoriesCount(part)>0) { 
                    return _contentManager
                        .Query(VersionOptions.Latest)
                        .Join<TerritoryPartRecord>()
                        .Where(tpr => tpr.Hierarchy.Id == part.Record.Id && tpr.ParentTerritory == null)
                        .List();
                } else {
                    return Enumerable.Empty<ContentItem>();
                }

            });
        }

        void RemoveTerritoriesInHierarchy(RemoveContentContext context, TerritoryHierarchyPart part) {
            // I need to only invoke this on the first level. Those TerritoryPart will Remove their children.
            foreach (var item in part.TopLevel) {
                _contentManager.Remove(item);
            }
        }

    }
}
