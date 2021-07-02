using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Settings;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Mvc.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.BundlesLocalizationExtension")]
    public class BundleAutocompleteLocalizationService : BundleAutocompleteServiceBase {

        public BundleAutocompleteLocalizationService(
            IContentManager contentManager,
            UrlHelper url)
            : base(contentManager, url) { }

        public override BundleViewModel BuildEditorViewModel(BundlePart part) {
            var bundleProductQuantities = part.ProductQuantities.ToDictionary(pq => pq.ProductId, pq => pq.Quantity);
            var ids = part.ProductQuantities.Select(x => x.ProductId);
            return new BundleViewModel {
                Products = GetProductParts(ids)
                    .Where(p => ConsiderProductValid(p, part))
                    .Select(
                        p => {
                            var id = p.ContentItem.Id;
                            return new ProductEntry {
                                ProductId = id,
                                Product = p,
                                Quantity = bundleProductQuantities.ContainsKey(id) ? bundleProductQuantities[id] : 0,
                                DisplayText = _contentManager.GetItemMetadata(p).DisplayText + ValidLocalization(part, p.ContentItem)

                            };
                        }
                    )
                    .OrderBy(vm => vm.DisplayText)
                    .ToList(),
                BundlePart = part
            };
        }

        protected string ValidLocalization(BundlePart part, ContentItem ci) {
            var locPart = part.ContentItem.As<LocalizationPart>();
            var lPart = ci.As<LocalizationPart>();
            if (lPart != null && lPart.Culture != null &&
                !string.IsNullOrWhiteSpace(lPart.Culture.Culture)) {
                if (lPart.Culture != locPart.Culture) {
                    return T(" ({0})", lPart.Culture.Culture).Text;
                }
            }
            else {
                return T(" (culture undefined)").Text;
            }
            return string.Empty;
        }

        public override List<ProductEntryAutocomplete> GetProducts(int contentItemId, string searchText, List<int> excludedProductIds) {
            // verify setting
            var ci = _contentManager.Get(contentItemId, VersionOptions.Latest);
            var part = ci.As<BundlePart>();
            var lPartBundle = part?.ContentItem.As<LocalizationPart>();


            IHqlQuery listProducts = ProductsQuery(searchText, excludedProductIds);
            if (lPartBundle != null && lPartBundle.Culture != null) {
                var settings = part.TypePartDefinition.Settings.GetModel<BundleProductLocalizationSettings>();
                if ((settings.TryToLocalizeProducts && settings.RemoveProductsWithoutLocalization) ||
                    settings.HideProductsFromEditor) {

                    var localizationAlias = "localizationPartRecord";
                    Action<IAliasFactory> localizationRecordAlias = x => x.ContentPartRecord<LocalizationPartRecord>().Named(localizationAlias);
                    Action<IHqlExpressionFactory> localizationSearch = loc => loc.InsensitiveLikeSpecificAlias(localizationAlias, "CultureId", lPartBundle.Culture.Id.ToString(), HqlMatchMode.Exact);

                    listProducts = listProducts
                        .Join(localizationRecordAlias)
                        .Where(a => a.ContentItem(), localizationSearch);
                }
            }

            return listProducts
                  .List()
                  .Select(x => new ProductEntryAutocomplete {
                      ProductId = x.Id,
                      EditUrl = _url.ItemEditUrl(x),
                      Quantity = 1,
                      DisplayText = _contentManager.GetItemMetadata(x).DisplayText,
                      Sku = x.As<ProductPart>() != null ? x.As<ProductPart>().Sku : string.Empty,
                      Lang = GetLang(x,lPartBundle,false),
                      DifferentLang = GetLang(x,lPartBundle,true),

                      HasPublished = x.HasPublished()
                  })
                .ToList();
        }

        private string GetLang(ContentItem ci, LocalizationPart lPartBundle, bool onlyDifferent) {
            var lang = T("culture undefined").Text;

            if (!string.IsNullOrWhiteSpace(ci.As<LocalizationPart>()?.Culture?.Culture)) {
                lang = ci.As<LocalizationPart>().Culture.Culture;
            }

            // check if return only lang of bundle or all language
            if (onlyDifferent) {
                // if bundle not have localization return all language
                if (!string.IsNullOrWhiteSpace(lPartBundle?.Culture?.Culture) 
                    && lang == lPartBundle.Culture.Culture) {
                    return string.Empty;
                }
                else {
                    return lang;
                }
            }
            else {
                return lang;
            }
        }
    }
}
