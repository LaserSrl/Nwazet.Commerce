using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Commerce")]
    public class InventoryPart : ContentPart<InventoryPartRecord> {
        /*
         Inventory discussion.
         This part will be used as "base" upon which services and providers will weld their
         own stuff. Basically, this part will store the "unmanaged" inventory, which is the
         one the merchant will manage themselves and for which there is no need for greater
         levels of detail/control. This part by itself supports the most basic scenarios, 
         (i.e. the ones supported so far with the properties in the ProductPart).
         More advanced scenarios, such as those where the inventory will depend on product 
         variations, and or where there are several warehouses that need to be managed,
         will become possible by welding services and other parts onto this one.
             */
        public int Inventory {
            get { return Retrieve(r => r.Inventory); }
            set { Store(r => r.Inventory, value); }
        }

        public string OutOfStockMessage {
            get { return Retrieve(r => r.OutOfStockMessage); }
            set { Store(r => r.OutOfStockMessage, value); }
        }

        public bool AllowBackOrder {
            get { return Retrieve(r => r.AllowBackOrder); }
            set { Store(r => r.AllowBackOrder, value); }
        }

        public int MinimumOrderQuantity {
            get {
                var minimumOrderQuantity = Retrieve(r => r.MinimumOrderQuantity);
                return minimumOrderQuantity > 1 ? minimumOrderQuantity : 1;
            }
            set {
                var minimumOrderQuantity = value > 1 ? value : 1;
                Store(r => r.MinimumOrderQuantity, minimumOrderQuantity);
            }
        }
        /// <summary>
        /// Back reference to service used for inventory operations.
        /// This is set when the InventoryPart is Activated.
        /// </summary>
        public IProductInventoryService ProductInventoryService { get; set; }
    }
}
