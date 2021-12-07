using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.Settings.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
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
        private readonly ICultureManager _cultureManager;
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IProductCombinationService _productCombinationService;

        public CombinationContainerPartHandler(
            IRepository<CombinationContainerPartRecord> repository,
            IContentManager contentManager,
            ILocalizationService localizationService,
            ICultureManager cultureManager,
            IProductAttributeAdminServices productAttributeAdminServices,
            IProductCombinationService productCombinationService) {

            _contentManager = contentManager;
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _productAttributeAdminServices = productAttributeAdminServices;
            _productCombinationService = productCombinationService;

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


        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<ProductAttributePart>();
            if (part != null) {
                context.Metadata.Identity.Add("AttributeName", part.TechnicalName);
            }
        }

        private IEnumerable<LocalizationPart> GetEditorLocalizations(LocalizationPart part) {
            return _localizationService.GetLocalizations(part.ContentItem, VersionOptions.Latest)
                .Where(c => c.Culture != null)
                .ToList();
        }
        private List<string> RetrieveMissingCultures(LocalizationPart part, bool excludePartCulture) {
            var editorLocalizations = GetEditorLocalizations(part);
            var cultures = _cultureManager
                .ListCultures()
                .Where(s => editorLocalizations.All(l => l.Culture.Culture != s))
                .ToList();
            if (excludePartCulture) {
                cultures.Remove(part.Culture.Culture);
            }
            return cultures;
        }

        // is correct?
        protected override void BuildEditorShape(BuildEditorContext context) {
            // case new translation of contentitem
            var localizationPart = context.ContentItem.As<LocalizationPart>();
            if (localizationPart == null || localizationPart.Culture != null || context.ContentItem.As<LocalizationPart>().MasterContentItem == null || context.ContentItem.As<LocalizationPart>().MasterContentItem.Id == 0) {
                return;
            }
            var part = context.ContentItem.Parts.Where(p => p.PartDefinition.Name == "CombinationContainerPart");
            if (part == null)
                return; // contentitem without taxonomy
            base.BuildEditorShape(context);
            var missingCultures = RetrieveMissingCultures(localizationPart, localizationPart.Culture != null);

            var vm = CreateVM(context.ContentItem.As<CombinationContainerPart>());
            var Prefix = "CombinationContainerPart";

            foreach (var missingCulture in missingCultures) {
                // get list of attributes we'll be able to use for combinations
                var allAttributes = _productAttributeAdminServices
                        .GetAllProductAttributeParts()
                        .Where(a => _localizationService.GetContentCulture(a.ContentItem) == missingCulture);

                vm.AllAttributeParts = allAttributes;

                var templateShape = context.New.EditorTemplate(
                    TemplateName: "Parts/Combinations/CombinationContainerPart",
                    Model: vm,
                    Prefix: Prefix
                );

                context.Shape.Parts_CombinationContainerPart_Editor = templateShape;
            }           
        }


        private CombinationContainerPartEditViewModel CreateVM(CombinationContainerPart part) {
            var partSettings = part.TypePartDefinition.Settings.GetModel<CombinationContainerPartSettings>();
            // Get existing combinations
            var currentCombinationRecords = part?.Record?.CombinationPartRecords ?? Enumerable.Empty<CombinationPartRecord>();
            var combinationContents = _contentManager
                .GetMany<CombinationPart>(currentCombinationRecords.Select(cpr => cpr.Id), VersionOptions.Latest, QueryHints.Empty);
            var comboTitles = new Dictionary<int, string>();
            foreach (var combo in combinationContents) {
                comboTitles.Add(
                    combo.Id,
                    _productCombinationService.CombinationDisplayText(combo));
            }
            return new CombinationContainerPartEditViewModel() {
                Part = part,
                CurrentCombinations = combinationContents,
                CombinationTitles = comboTitles,
                CombinationTypeName = partSettings?.CombinationTypeName ?? string.Empty
            };
        }


    }
}
