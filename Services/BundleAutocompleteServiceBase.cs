using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Bundles")]
    public abstract class BundleAutocompleteServiceBase : IBundleAutocompleteService {
        protected readonly IContentManager _contentManager;
        protected UrlHelper _url;

        public BundleAutocompleteServiceBase(
            IContentManager contentManager,
            UrlHelper url) {

            _contentManager = contentManager;
            _url = url;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected virtual bool ConsiderProductValid(IContent prod, BundlePart part) {
            return true;
        }

        protected IEnumerable<ProductPart> GetProductParts(IEnumerable<int> ids) {
            return _contentManager.GetMany<ProductPart>(ids, VersionOptions.Latest, QueryHints.Empty)
                    .Where(p => !p.Has<BundlePart>());
        }

        public virtual BundleViewModel BuildEditorViewModel(BundlePart part) {
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
                                DisplayText = _contentManager.GetItemMetadata(p).DisplayText
                            };
                        }
                    )
                    .OrderBy(vm => vm.DisplayText)
                    .ToList(),
                BundlePart = part
            };
        }
        
        protected IHqlQuery ProductsQuery(string searchText, List<int> excludedProductIds) {
            if (excludedProductIds == null)
                excludedProductIds = new List<int>();
            var productAlias = "productPartVersionRecord";
            var titleAlias = "titlePartRecord";
            Action<IAliasFactory> productPartRecordAlias = x => x.ContentPartRecord<ProductPartVersionRecord>().Named(productAlias);
            Action<IAliasFactory> titlePartRecordAlias = x => x.ContentPartRecord<TitlePartRecord>().Named(titleAlias);
            Action<IHqlExpressionFactory> titleSearch = title => title.InsensitiveLikeSpecificAlias(titleAlias, "Title", searchText, HqlMatchMode.Anywhere);
            Action<IHqlExpressionFactory> skuSearch = sku => sku.InsensitiveLikeSpecificAlias(productAlias, "Sku", searchText, HqlMatchMode.Anywhere);
            Action<IHqlExpressionFactory> idNotExcluded = p => p.Gt("Id", 0);
            if (excludedProductIds.Count > 0)
                idNotExcluded = p => p.Not(q => q.In("Id", excludedProductIds.ToArray()));

            Action<IHqlExpressionFactory> conditionTitle = title => title.And(idNotExcluded, titleSearch);
            Action<IHqlExpressionFactory> conditionSku = sku => sku.And(idNotExcluded, skuSearch);

            return _contentManager.HqlQuery()
                .ForVersion(VersionOptions.Latest)
                .Join(productPartRecordAlias)
                .Join(titlePartRecordAlias)
                // changed the incorrect condition not (id in (ids) and title like '%%' or sku like '%%')
                // with the correct condition for sql (id in (ids) and title like '%%' or id in (ids) and sku like '%%')
                .Where(a => a.ContentItem(),
                   x => x.Or(conditionTitle, conditionSku))
               .OrderBy(titlePartRecordAlias, o => o.Asc("Title"));
        }

        public virtual List<ProductEntryAutocomplete> GetProducts(int contentItemId,string searchText, List<int> excludedProductIds) {          
            return ProductsQuery(searchText, excludedProductIds)
               .List()
               .Select(x => new ProductEntryAutocomplete {
                   ProductId = x.Id,
                   EditUrl = _url.ItemEditUrl(x),
                   Quantity = 1,
                   DisplayText = _contentManager.GetItemMetadata(x).DisplayText,
                   Sku = x.As<ProductPart>() != null ? x.As<ProductPart>().Sku : string.Empty,
                   HasPublished = x.HasPublished()
               })
               .ToList();
        }
    }
    
}
