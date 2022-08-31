using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Mvc.Html;
using Orchard.OutputCache.Services;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nwazet.Commerce.Handlers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IProductCombinationService _productCombinationService;
        private readonly ICacheService _cacheService;
        private readonly ILocalizationService _localizationService;
        private readonly IEnumerable<ICombinationDetailProvider> _combinationDetailProviders;
        private readonly WorkContext _workContext;
        
        protected UrlHelper _url;

        // populated in case of duplicates
        private bool combinationIsDuplicated = false;

        public CombinationPartHandler(
            IOrchardServices orchardServices,
            IRepository<CombinationPartRecord> repository,
            IContentManager contentManager,
            IProductCombinationService productCombinationService,
            ICacheService cacheService,
            ILocalizationService localizationService,
            UrlHelper url,
            IEnumerable<ICombinationDetailProvider> combinationDetailProviders,
            IWorkContextAccessor workContextAccessor) {

            Services = orchardServices;
            _contentManager = contentManager;
            _productCombinationService = productCombinationService;
            _cacheService = cacheService;
            _localizationService = localizationService;
            _url = url;
            _combinationDetailProviders = combinationDetailProviders;

            _workContext = workContextAccessor.GetContext();

            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(repository));

            //Lazyfield setters
            OnInitializing<CombinationPart>(PropertySetHandlers);
            OnLoading<CombinationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<CombinationPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

            // check that the container is correct if not, replace it with the correct language container
            OnUpdated<CombinationPart>((context, part) => CheckCombinationContainer(context, part));

            // if the combination is duplicated, delete the publication
            OnPublishing<CombinationPart>((context, part) => CheckCombinationDuplicated(context, part));

            // When combinations get updated, we may wish to have something to evict cached
            // stuff about their containers
            OnPublished<CombinationPart>((context, part) => InvalidateParentCache(part));
            OnUnpublished<CombinationPart>((context, part) => InvalidateParentCache(part));
            OnRemoved<CombinationPart>((context, part) => InvalidateParentCache(part));
            OnDestroyed<CombinationPart>((context, part) => InvalidateParentCache(part));

            // When loading the CombinationPart, I we want the content type to be represented by its container's.
            // This is needed, for instance, to evaluate coupon or shipping criteria.
            OnLoaded<CombinationPart>((ctx, part) => WeldContainer(part));
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T;

        void InvalidateParentCache(CombinationPart part) {
            // Cache items directly marked for this ContentItem are evicted elsewhere
            // (see Orchard.OutputCache.Handlers.CacheItemInvalidationHandler). Here
            // we should make sure the container for the CombinationPart is evicted as
            // well, since that is generally what's used for the frontend.
            var container = part.CombinationContainerPart;
            if (container != null) {
                _cacheService.RemoveByTag(container.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<CombinationPart>();

            if (part != null) {
                if (part.CombinationContainerPart != null) {
                    var containerMeta = _contentManager.GetItemMetadata(part.CombinationContainerPart);
                    context.Metadata.DisplayText =
                        $"{containerMeta.DisplayText} ({_productCombinationService.CombinationDisplayText(part)})";

                    context.Metadata.DisplayRouteValues = containerMeta.DisplayRouteValues;

                } else {
                    context.Metadata.DisplayText =
                        $"{_productCombinationService.CombinationDisplayText(part)}";
                }
            }
        }

        static void PropertySetHandlers(
            InitializingContentContext context, CombinationPart part) {

            part.CombinationContainerPartField.Setter(container => {
                part.Record.CombinationContainerPartRecord =
                    container.As<CombinationContainerPart>().Record;
                return container;
            });

            // call the setters in case a value had already been set
            if (part.CombinationContainerPartField.Value != null) {
                part.CombinationContainerPartField.Value = part.CombinationContainerPartField.Value;
            }
        }

        void LazyLoadHandlers(CombinationPart part) {
            part.CombinationContainerPartField.Loader(() => {
                if (part.Record.CombinationContainerPartRecord != null) {
                    return _contentManager
                        .Get<CombinationContainerPart>(part.Record.CombinationContainerPartRecord.Id,
                            VersionOptions.Latest, QueryHints.Empty);
                } else {
                    return null;
                }

            });
        }

        void CheckCombinationContainer(UpdateContentContext context, CombinationPart part) {

            var container = part.CombinationContainerPart;
            var ci = container.ContentItem;
            if (ci.Has<LocalizationPart>() && ci.As<LocalizationPart>().Culture != null) {
                var contentCulture = context.ContentItem.As<LocalizationPart>().Culture != null ? context.ContentItem.As<LocalizationPart>().Culture : null;
                if (contentCulture != container.ContentItem.As<LocalizationPart>().Culture) {
                    // check translation of CombinationContainerPart
                    var realCombinationContainerPart = _localizationService.GetLocalizations(ci)
                        .FirstOrDefault(l => l.As<LocalizationPart>().Culture == contentCulture);
                    if (realCombinationContainerPart != null &&
                        realCombinationContainerPart.ContentItem.As<CombinationContainerPart>() != null) {
                        // save the container in the correct language
                        part.CombinationContainerPartField.Value = realCombinationContainerPart.ContentItem.As<CombinationContainerPart>();

                        // check that the combination is not already present in the new container
                        var newCurrentCombinations = part.CombinationContainerPart
                            .CombinationParts
                            .Select(cp => CombinationPart.DeserializeCombinations(cp));
                        foreach (var comb in newCurrentCombinations) {
                            if (!part.ProductAttributeValues.Any(a =>
                                    !comb.Any(com => com.AttributeId == a.AttributeId && com.AttributeValue == a.AttributeValue))) {
                                combinationIsDuplicated = true;
                                Services.Notifier.Error(T("The selected combination already exists."));
                            }
                        }
                        Services.Notifier.Information(T("Your combination has been moved under the <a href=\"{0}\">{1}</a",
                            _url.ItemEditUrl(part.CombinationContainerPart),
                            _contentManager.GetItemMetadata(part.CombinationContainerPart).DisplayText));
                    }
                }
            }
        }

        void CheckCombinationDuplicated(PublishContentContext context, CombinationPart part) {
            // if the published combination is a duplicate disable publish
            // fe is shown that it is a duplicate
            if (combinationIsDuplicated) {
                context.Cancel = true;
            }
        }

        void WeldContainer(CombinationPart part) {
            if (!AdminFilter.IsApplied(_workContext.HttpContext.Request.RequestContext)) {
                var container = part.CombinationContainerPart;
                if (container != null) {
                    var containerContentType = container.ContentItem.ContentType;
                    var originalContentType = part.ContentItem.ContentType;
                    part.ContentItem.ContentType = containerContentType;

                    // Weld every configured field to the ContentItem (if it's not already there).
                    foreach (var p in container.ContentItem.Parts) {
                        foreach (var f in p.Fields) {
                            if ((part.Fields.FirstOrDefault(fld => fld.Name == f.Name) == null)
                                && f.PartFieldDefinition.Settings
                                    .ContainsKey("ContentFieldCombinationWeldingSettings.WeldToCombination")) {
                                var weldField = false;
                                bool.TryParse(f.PartFieldDefinition.Settings["ContentFieldCombinationWeldingSettings.WeldToCombination"], out weldField);
                                if (weldField) {
                                    if (p.PartDefinition.Name.Equals(containerContentType, StringComparison.OrdinalIgnoreCase)) {
                                        // If it's the "standard" {ContentType} part, fields are just weld to the CombinationPart.
                                        part.Weld(f);
                                    } else {
                                        // I check for a part with the same name of the part the field is into.
                                        var partToWeldFieldTo = part.ContentItem.Parts
                                            .FirstOrDefault(pa => pa.PartDefinition.Name == p.PartDefinition.Name);
                                        if (partToWeldFieldTo != null) {
                                            if ((part.Fields.FirstOrDefault(fld => fld.Name == f.Name) == null)) {
                                                // If the part is found, just weld the field to it.
                                                partToWeldFieldTo.Weld(f);
                                            }
                                        } else {
                                            // Create a new ContentPart with the same original name to weld the field to.
                                            ContentPart newPart = new ContentPart();
                                            newPart.TypePartDefinition = new ContentTypePartDefinition(
                                                new ContentPartDefinition(p.PartDefinition.Name),
                                                new SettingsDictionary()
                                            );
                                            newPart.Weld(f);
                                            part.ContentItem.Weld(newPart);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // Call the AfterLoaded of every ICombinationDetailProvider for specific actions to be completed after welding fields.
                    foreach (var cdp in _combinationDetailProviders) {
                        cdp.AfterLoaded(part);
                    }
                }
            }
        }
    }
}
