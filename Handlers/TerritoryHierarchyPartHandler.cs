using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public TerritoryHierarchyPartHandler(
            IRepository<TerritoryHierarchyPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            
            //Lazyfield setters
            OnInitializing<TerritoryHierarchyPart>(PropertySetHandlers);
            OnLoaded<TerritoryHierarchyPart>(LazyLoadHandlers);
        }

        static void PropertySetHandlers(
            InitializingContentContext context, TerritoryHierarchyPart part) {

            part.TerritoriesField.Setter(value => {
                var actualItems = value.Where(ci => ci.As<TerritoryPart>() != null);
                part.Record.Territories = actualItems.Any() ? 
                    actualItems.Select(ci => ci.As<TerritoryPart>().Record).ToList():
                    new List<TerritoryPartRecord>();
                return actualItems;
            });

            //call the setter in case a value had already been set
            if (part.TerritoriesField.Value != null) {
                part.TerritoriesField.Value = part.TerritoriesField.Value;
            }
        }

        void LazyLoadHandlers(
            LoadContentContext context, TerritoryHierarchyPart part) {

            part.TerritoriesField.Loader(() => {
                if (part.Record.Territories != null && part.Record.Territories.Any()) {
                    return _contentManager
                        .GetMany<IContent>(part.Record.Territories.Select(tpr => tpr.ContentItemRecord.Id),
                            VersionOptions.Latest, null);
                } else {
                    return Enumerable.Empty<ContentItem>();
                }

            });
        }

    }
}
