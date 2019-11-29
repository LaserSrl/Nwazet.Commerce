using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Commerce")]
    public class ProductPartHandler : ContentHandler {
        private readonly IProductService _productService;
        private readonly IProductPriceService _productPriceService;
        private readonly IProductInventoryService _productInventoryService;

        public ProductPartHandler(
            IRepository<ProductPartRecord> repository,
            IProductService productService,
            IProductPriceService productPriceService,
            IProductInventoryService productInventoryService) {

            _productService = productService;
            _productPriceService = productPriceService;
            _productInventoryService = productInventoryService;

            Filters.Add(StorageFilter.For(repository));

            OnActivated<ProductPart>((ctx, part) => {
                part.ProductService = _productService;
                part.ProductPriceService = _productPriceService;
                part.ProductInventoryService = _productInventoryService;
            });
        }
        
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<ProductPart>();

            if (part != null) {
                context.Metadata.Identity.Add("Sku", part.Sku);
            }
        }
    }
}
