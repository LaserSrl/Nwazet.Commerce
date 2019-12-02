using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    public class InventoryEditViewModel {

        public InventoryEditViewModel() { }

        public InventoryEditViewModel(InventoryPart part) {
            Inventory = part.Inventory;
            OutOfStockMessage = part.OutOfStockMessage;
            AllowBackOrder = part.AllowBackOrder;
            MinimumOrderQuantity = part.MinimumOrderQuantity;
        }

        public int Inventory { get; set; }
        public string OutOfStockMessage { get; set; }
        public bool AllowBackOrder { get; set; }
        public int MinimumOrderQuantity { get; set; }
    }
}
