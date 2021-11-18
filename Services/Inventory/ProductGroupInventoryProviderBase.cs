using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Inventory {
    public abstract class ProductGroupInventoryProviderBase : IProductGroupInventoryProvider {

        protected readonly IWorkContextAccessor _workContextAccessor;

        protected ProductGroupInventoryProviderBase(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;
        }

        public virtual IEnumerable<ProductPart> AddProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> previous) {
            // By default, we are not going to add any product
            return Enumerable.Empty<ProductPart>();
        }

        public virtual IEnumerable<ProductPart> FilterProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> products) {
            // By default, we are not going to remove any product
            return Enumerable.Empty<ProductPart>();
        }

        public virtual IEnumerable<ProductPart> AddProductsWithInventoryIssues() {
            // By default, we are not going to add any product
            return Enumerable.Empty<ProductPart>();
        }


        public virtual IEnumerable<ProductPart> FilterProductsWithInventoryIssues(
            IEnumerable<ProductPart> products) {
            // By default, we are not going to remove any product
            return Enumerable.Empty<ProductPart>();
        }


        protected int GetInventory(ProductPart part) {
            IBundleService bundleService;
            var inventory = part.As<InventoryPart>()?.Inventory ?? 0;
            // We resolve this stuff rather than injecting it because if
            // the Bundles feature is not active Autofac would fail injecting it 
            if (_workContextAccessor.GetContext().TryResolve(out bundleService)
                && part.Has<BundlePart>()) {
                var bundlePart = part.As<BundlePart>();
                var ids = bundlePart.ProductIds.ToList();
                if (!ids.Any()) return 0;

                var productQuantitiesFor =
                    bundleService
                        .GetProductQuantitiesFor(bundlePart);
                if (!productQuantitiesFor.Any()) return 0;

                inventory = productQuantitiesFor
                    .Min(p => p.Product.Inventory / p.Quantity);
            }
            return inventory;
        }
    }
}
