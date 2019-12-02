using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Commerce")]
    public class InventoryPartRecord : ContentPartRecord {
        public virtual int Inventory { get; set; }
        public virtual string OutOfStockMessage { get; set; }
        public virtual bool AllowBackOrder { get; set; }
        public virtual int MinimumOrderQuantity { get; set; }
    }
}
