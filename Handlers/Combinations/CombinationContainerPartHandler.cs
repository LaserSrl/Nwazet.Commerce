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

namespace Nwazet.Commerce.Handlers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;

        public CombinationContainerPartHandler(
            IRepository<CombinationContainerPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));

            // Lazyfield setters
            OnInitializing<CombinationContainerPart>(PropertySetHandlers);
            OnLoading<CombinationContainerPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<CombinationContainerPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

            // When we are unpublishing/deleting a container, we should do the same to its combinations
            OnUnpublishing<CombinationContainerPart>(UnpublishCombinations);
            OnRemoving<CombinationContainerPart>(RemoveCombinations);
            OnDestroying<CombinationContainerPart>(DestroyCombinations);
        }

        void UnpublishCombinations(PublishContentContext context, CombinationContainerPart part) {
            foreach (var combination in part.CombinationParts) {
                _contentManager.Unpublish(combination.ContentItem);
            }
        }

        void RemoveCombinations(RemoveContentContext context, CombinationContainerPart part) {
            foreach (var combination in part.CombinationParts) {
                _contentManager.Remove(combination.ContentItem);
            }
        }

        void DestroyCombinations(DestroyContentContext context, CombinationContainerPart part) {
            foreach (var combination in part.CombinationParts) {
                _contentManager.Destroy(combination.ContentItem);
            }
        }

        void LazyLoadHandlers(CombinationContainerPart part) {
            part.CombinationPartsField.Loader(() => {
                if (part.Record.CombinationPartRecords != null) {
                    if (part.Record.CombinationPartRecords.Any()) {
                        return _contentManager
                            .GetMany<CombinationPart>(
                                part.Record.CombinationPartRecords.Select(r => r.Id),
                                VersionOptions.Latest,
                                QueryHints.Empty)
                            .ToList();
                    }
                }
                return new List<CombinationPart>();
            });
        }

        static void PropertySetHandlers(InitializingContentContext context, CombinationContainerPart part) {
            part.CombinationPartsField.Setter(parts => {
                part.Record.CombinationPartRecords =
                    parts.Select(pa => pa.Record).ToList();
                return parts;
            });

            // call the setters in case a value had already been set
            if (part.CombinationPartsField.Value != null) {
                part.CombinationPartsField.Value = part.CombinationPartsField.Value;
            }
        }
    }
}
