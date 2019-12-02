using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.Commerce")]
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
