using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.Settings.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;
        
        public CombinationContainerPartHandler(
            IRepository<CombinationContainerPartRecord> repository,
            IContentManager contentManager,
            ILocalizationService localizationService,
            INotifier notifier) {

            _contentManager = contentManager;
            _localizationService = localizationService;
            _notifier = notifier;

            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(repository));

            // Lazyfield setters
            OnInitializing<CombinationContainerPart>(PropertySetHandlers);
            OnLoading<CombinationContainerPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<CombinationContainerPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

            OnPublishing<CombinationContainerPart>((context, part) => ImportLocalizedCombination(context, part));

            // When we are unpublishing/deleting a container, we should do the same to its combinations
            OnUnpublishing<CombinationContainerPart>(UnpublishCombinations);
            OnRemoving<CombinationContainerPart>(RemoveCombinations);
            OnDestroying<CombinationContainerPart>(DestroyCombinations);
        }

        public Localizer T { get; set; }

        void ImportLocalizedCombination(PublishContentContext context, CombinationContainerPart part) {
            // When saving a Container translation I automatically move to the container any combination
            // in the corresponding language associated to the other translations
            var combinationsMoved = false;
            // check the current container be localized
            if (context.ContentItem.Has<LocalizationPart>()) {
                var containerCulture = context.ContentItem.As<LocalizationPart>().Culture;
                if (containerCulture != null) {
                    // get all localizations of the current container (except the current container itself)
                    var containerLocalizations = _localizationService
                        .GetLocalizations(context.ContentItem)
                        .Where(lp => lp.Id != context.ContentItem.Id);
                    foreach (var localizationPart in containerLocalizations) {
                        var parentCulture = localizationPart.Culture;

                        var container = localizationPart.ContentItem.As<CombinationContainerPart>();
                        if (container != null) {
                            // Get existing combinations
                            var combinationContents = container.CombinationParts
                                .Where(c => {
                                    var locPart = c.As<LocalizationPart>();
                                    if (locPart == null) {
                                        return false;
                                    }
                                    var cult = locPart.Culture;
                                    return cult != null && cult != parentCulture && cult == containerCulture;
                                })
                                .ToList();
                            combinationsMoved = combinationContents.Any();
                            // changed container
                            foreach (var comb in combinationContents) {
                                comb.CombinationContainerPartField.Value = part.ContentItem.As<CombinationContainerPart>();
                            }
                        }
                    }
                }
            }

            if (combinationsMoved) { 
                _notifier.Add(NotifyType.Information, T("Combinations in the chosen language have been automatically moved from the other translations."));
            }
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


        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<ProductAttributePart>();
            if (part != null) {
                context.Metadata.Identity.Add("AttributeName", part.TechnicalName);
            }
        }
    }
}
