using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    public class InventoryPart : ContentPart<InventoryPartRecord> {

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
    }
}
