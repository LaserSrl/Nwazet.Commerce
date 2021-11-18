using System.Collections.Generic;
using System.Linq;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.Services.Inventory {
    public class ProductInventoryService : IProductInventoryService {
        protected readonly IWorkContextAccessor _workContextAccessor;
        protected readonly IContentManager _contentManager;
        protected readonly IEnumerable<IProductGroupInventoryProvider> _productGroupInventoryProviders;

        public ProductInventoryService(
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager,
            IEnumerable<IProductGroupInventoryProvider> productGroupInventoryProviders) {

            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _productGroupInventoryProviders = productGroupInventoryProviders;
        }

        public IEnumerable<ProductPart> GetProductsWithSameInventory(ProductPart part) {

            var products = new List<ProductPart>();
            foreach (var provider in _productGroupInventoryProviders) {
                products.AddRange(provider.AddProductsWithSameInventory(part, products));
                // TODO: should we handle duplicates?
            }
            foreach (var provider in _productGroupInventoryProviders) {
                var toRemove = provider.FilterProductsWithSameInventory(part, products);
                products.RemoveAll(p => 
                    toRemove.Any(pp => 
                        pp.Id == p.Id 
                        && pp.ContentItem.VersionRecord.Id == p.ContentItem.VersionRecord.Id));
            }

            return products;
        }

        /// <summary>
        /// Uses the inventory of the ProductPart parameter to update the ProductParts whose inventory
        /// has to be kept in synch with the parameter's.
        /// </summary>
        /// <param name="part">The ProductPart whose inventory will be copied over.</param>
        public void SynchronizeInventories(ProductPart part) {
            // Synchronize inventory between Latest and Published versions
            int inv = GetInventory(part);
            foreach (var pp in
               GetProductsWithSameInventory(part)
                   .Where(pa => GetInventory(pa) != inv)) { //condition to avoid infinite recursion
                SetInventory(pp, GetInventory(part)); //call methods from base class
            }
            //Synchronize the inventory for the eventual bundles that contain the product
            IBundleService bundleService;
            if (_workContextAccessor.GetContext().TryResolve(out bundleService)) {
                var affectedBundles = _contentManager.Query<BundlePart, BundlePartRecord>()
                    .Where(b => b.Products.Any(p => p.ContentItemRecord.Id == part.Id))
                    .WithQueryHints(new QueryHints().ExpandParts<ProductPart>())
                    .List();
                foreach (var bundle in affectedBundles.Where(b => b.ContentItem.As<ProductPart>() != null)) {
                    var prod = bundle.ContentItem.As<ProductPart>();
                    SetInventory(prod, GetInventory(prod));
                }
            }
        }

        public int SetInventory(ProductPart part, int inventoryValue) {
            part.As<InventoryPart>().Inventory = inventoryValue;
            SynchronizeInventories(part);
            return part.Inventory;
        }

        public int UpdateInventory(ProductPart part, int inventoryChange) {
            part.As<InventoryPart>().Inventory += inventoryChange;
            SynchronizeInventories(part);
            return part.Inventory;
        }

        public int GetInventory(InventoryPart part) {
            IBundleService bundleService;
            var inventory = part.Inventory;
            if (_workContextAccessor.GetContext().TryResolve(out bundleService) && part.Has<BundlePart>()) {
                var bundlePart = part.As<BundlePart>();
                var ids = bundlePart.ProductIds.ToList();
                if (!ids.Any()) return 0;
                inventory =
                    bundleService
                        .GetProductQuantitiesFor(bundlePart)
                        .Min(p => p.Product.Inventory / p.Quantity);
            }
            return inventory;
        }

        public int GetInventory(ProductPart part) {
            IBundleService bundleService;
            var inventory = part.As<InventoryPart>()?.Inventory ?? 0;
            if (_workContextAccessor.GetContext().TryResolve(out bundleService) && part.Has<BundlePart>()) {
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

        public IEnumerable<ProductPart> GetProductsWithInventoryIssues() {
            var products = new List<ProductPart>();
            foreach (var provider in _productGroupInventoryProviders) {
                products.AddRange(provider.AddProductsWithInventoryIssues());
                // TODO: should we handle duplicates?
            }
            foreach (var provider in _productGroupInventoryProviders) {
                var toRemove = provider.FilterProductsWithInventoryIssues(products);
                products.RemoveAll(p =>
                    toRemove.Any(pp =>
                        pp.Id == p.Id
                        && pp.ContentItem.VersionRecord.Id == p.ContentItem.VersionRecord.Id));
            }

            return products;
        }
    }
}
