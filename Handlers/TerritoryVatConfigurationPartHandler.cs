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
    public class TerritoryVatConfigurationPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public TerritoryVatConfigurationPartHandler(
            IRepository<TerritoryVatConfigurationPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));

            // Lazyfield setters and loaders
            OnInitializing<TerritoryVatConfigurationPart>(PropertySetHandlers);
            OnLoading<TerritoryVatConfigurationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<TerritoryVatConfigurationPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

        }

        protected override void Activating(ActivatingContentContext context) {
            // Attach this part wherever there is a TerritoryPart
            if (context.Definition.Parts.Any(pa => pa.PartDefinition.Name == "TerritoryPart")) {
                // the ContentItem we are activating is a territory
                context.Builder.Weld<TerritoryVatConfigurationPart>();
            }
            base.Activating(context);
        }

        static void PropertySetHandlers(
            InitializingContentContext context, TerritoryVatConfigurationPart part) {

            part.VatConfigurationsField.Setter(value => {
                return value
                    .Where(tup => tup
                        .Item1 // Item1 is the VatConfigurationPart
                        .Record
                        .TerritoryConfigurationIntersections
                        .Any(tvcir => tvcir.Territory == part.Record)
                    )
                    .ToList();
            });

            // call the setter in case a value had already been set
            if (part.VatConfigurationsField.Value != null) {
                part.VatConfigurationsField.Value = part.VatConfigurationsField.Value;
            }
        }

        void LazyLoadHandlers(TerritoryVatConfigurationPart part) {

            part.VatConfigurationsField.Loader(() => {
                if (part.Record.VatConfigurationIntersections != null
                    && part.Record.VatConfigurationIntersections.Any()) {
                    // IEnumerable<Tuple<A, B>> pairs = listA.Zip(listB, (a, b) => Tuple.Create(a, b));
                    return part.Record.VatConfigurationIntersections
                    .Zip(_contentManager
                        .GetMany<VatConfigurationPart>(part.Record.VatConfigurationIntersections
                            .Select(tvcir => tvcir.VatConfiguration.Id),
                            VersionOptions.Latest, QueryHints.Empty),
                        (a, b) => Tuple.Create(b, a.Rate));
                } else {
                    return Enumerable.Empty<Tuple<VatConfigurationPart, decimal>>();
                }

                return null;
            });
        }
    }
}
