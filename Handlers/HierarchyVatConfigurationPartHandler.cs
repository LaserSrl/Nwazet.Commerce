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
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class HierarchyVatConfigurationPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public HierarchyVatConfigurationPartHandler(
            IRepository<HierarchyVatConfigurationPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            
            // Lazyfield setters and loaders
            OnInitializing<HierarchyVatConfigurationPart>(PropertySetHandlers);
            OnLoading<HierarchyVatConfigurationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<HierarchyVatConfigurationPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            
        }

        protected override void Activating(ActivatingContentContext context) {
            // Attach this part wherever there is a TerritoryHierarchyPart
            if (context.Definition.Parts.Any(pa => pa.PartDefinition.Name == "TerritoryHierarchyPart")) {
                // the ContentItem we are activating is a hierarchy
                context.Builder.Weld<HierarchyVatConfigurationPart>();
            }
            base.Activating(context);
        }

        static void PropertySetHandlers(
            InitializingContentContext context, HierarchyVatConfigurationPart part) {
            
            part.VatConfigurationsField.Setter(value => {
                return value
                    .Where(tup => tup
                        .Item1 // Item1 is the VatConfigurationPart
                        .Record
                        .HierarchyConfigurationIntersections
                        .Any(hvcir => hvcir.Hierarchy == part.Record)
                    )
                    .ToList();
            });

            // call the setter in case a value had already been set
            if (part.VatConfigurationsField.Value != null) {
                part.VatConfigurationsField.Value = part.VatConfigurationsField.Value;
            }
        }

        void LazyLoadHandlers(HierarchyVatConfigurationPart part) {
            
            part.VatConfigurationsField.Loader(() => {
                if (part.Record.VatConfigurationIntersections != null
                    && part.Record.VatConfigurationIntersections.Any()) {
                    // IEnumerable<Tuple<A, B>> pairs = listA.Zip(listB, (a, b) => Tuple.Create(a, b));
                    return part.Record.VatConfigurationIntersections
                        .Zip(_contentManager
                            .GetMany<VatConfigurationPart>(part.Record.VatConfigurationIntersections
                                .Select(hvcir => hvcir.VatConfiguration.Id),
                                VersionOptions.Latest, QueryHints.Empty),
                            (a, b) => Tuple.Create(b, a.Rate));
                } else {
                    return Enumerable.Empty<Tuple<VatConfigurationPart, decimal>>();
                }
            });
        }
    }
}
