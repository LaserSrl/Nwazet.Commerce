using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Inventory;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Events {
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
            if (order != null && order.Items!= null) {
                // TODO: refine this for bundles and such
                // get the product contentItems corresponding to the items in the order
                var products = _contentManager.GetMany<InventoryControlPart>(
                    order.Items.Select(coi => coi.ProductId).Distinct(),
                    VersionOptions.Published,
                    QueryHints.Empty);
                // for every product in the order
                foreach (var prod in products.Where(p => !p.PreventAutomaticDecrease)) {
                    // if they are configured to have their inventory decrease automatically
                    // decrease their inventory by however many items are in the order
                    var quantity = order.Items
                        // this handles the case where the order has more "copies" of the
                        // same product (e.g. with different attributes).
                        .Where(coi => coi.ProductId == prod.Id)
                        .Sum(coi => coi.Quantity);
                    _productInventoryService.UpdateInventory(prod.As<ProductPart>(), -quantity);

                }
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
