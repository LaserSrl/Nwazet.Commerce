using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Inventory;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Events.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlOrderEventHandler : IOrderEventHandler {
        private readonly IContentManager _contentManager;
        private readonly IProductInventoryService _productInventoryService;

        public InventoryControlOrderEventHandler(
            IContentManager contentManager,
            IProductInventoryService productInventoryService) {

            _contentManager = contentManager;
            _productInventoryService = productInventoryService;
        }

        public void OnNewOrder(OrderPart order) {
            // begin with sanity checks
            if (order != null && order.Items != null && order.Items.Any()) {
                // We make a Dictionary<int, int> of product Ids, quantity to track for each
                // product Id how much we should decrease its corresponding inventory (if we
                // end up decreasing it)
                var quantities = new Dictionary<int, int>();
                foreach (var item in order.Items) {
                    if (quantities.ContainsKey(item.ProductId)) {
                        quantities[item.ProductId] += item.Quantity;
                    } 
                    else {
                        quantities.Add(item.ProductId, item.Quantity);
                    }
                }
                // We get all products in the Order.
                var products = _contentManager.GetMany<ProductPart>(
                    order.Items.Select(coi => coi.ProductId).Distinct(),
                    VersionOptions.Published,
                    QueryHints.Empty)
                    .ToList();
                // Some of those products, will actually be "containers" for other products.
                // This is the case for bundles. We have then to "expand" our query to those.
                if (products.Any(p => p.Is<BundlePart>())) {
                    var idsFromBundles = new List<int>();
                    foreach (var prod in products.Where(p => p.Is<BundlePart>())) {
                        var bundlePart = prod.As<BundlePart>();
                        // how many of this bundle are in the order?
                        var bundleQuantity = order.Items
                            .FirstOrDefault(ci => ci.ProductId == bundlePart.Id)
                            .Quantity;
                        idsFromBundles.AddRange(bundlePart.ProductIds);
                        // add quantities to dictionary
                        foreach (var item in bundlePart.ProductQuantities) {
                            if (quantities.ContainsKey(item.ProductId)) {
                                quantities[item.ProductId] += item.Quantity * bundleQuantity;
                            }
                            else {
                                quantities.Add(item.ProductId, item.Quantity * bundleQuantity);
                            }
                        }
                    }
                    // We processed all bundles, so now do a single query for all products found there,
                    // except the ones we already fetched earlier because they were already found in the
                    // order directly.
                    products.AddRange(_contentManager.GetMany<ProductPart>(
                        idsFromBundles.Distinct()
                            .Except(products.Select(pp => pp.Id)),
                        VersionOptions.Published,
                        QueryHints.Empty));
                }

                // In this handler we should be processing only products that have the 
                // InventoryControlPart. Out of those, some may be set to PreventAutomaticDecrease
                // of their inventory on order: we have nothing to do to those.
                var icParts = products
                    .Distinct(new ContentPartEqualityComparer())
                    .Where(pp => pp.Is<InventoryControlPart>())
                    .Select(pp => pp.As<InventoryControlPart>());
                // For those products we should actually be decreasing, determine the quantity:
                // we may have different "copies" of the same product (e.g. with different
                // attributes, or if the product is also in a bundle). This has been computed
                // and can be fetched from the dictionary.
                foreach (var prod in icParts.Where(p => !p.PreventAutomaticDecrease)) {
                    // if they are configured to have their inventory decrease automatically
                    // decrease their inventory by however many items are in the order
                    var quantity = 0;
                    if (quantities.TryGetValue(prod.Id, out quantity)) {
                        // testing that the key is in the dictionary is more of a sanity check than
                        // anything, because the way we built the dictionary is such that all products
                        // should be there.
                        _productInventoryService.UpdateInventory(prod.As<ProductPart>(), -quantity);
                    }
                }
            }
        }

        class ContentPartEqualityComparer : IEqualityComparer<ContentPart> {
            public bool Equals(ContentPart x, ContentPart y) {
                return x.Id == y.Id;
            }

            public int GetHashCode(ContentPart obj) {
                return
                    obj.ContentItem?.GetHashCode() ?? 0 +
                    obj.Id.GetHashCode();
            }
        }

        #region Not implemented IOrderEventHandler methods
        public void OnNewPayment(OrderPart order) {
            // nothing to do here
        }

        public void OnOrderError(OrderPart order, Dictionary<string, string> errors) {
            // nothing to do here
        }
        public void OnOrderStatusChanged(OrderPart order, string previousStatus) {
            // nothing to do here
        }

        public void OnOrderStatusChangedProduct(OrderPart order, ContentItem product, string previousStatus) {
            // nothing to do here
        }

        public void OnOrderTrackingUrlChanged(OrderPart order) {
            // nothing to do here
        }
        #endregion
    }
}
