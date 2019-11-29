using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    public class InventoryPartRecord : ContentPartRecord {
        public virtual int Inventory { get; set; }
        public virtual string OutOfStockMessage { get; set; }
        public virtual bool AllowBackOrder { get; set; }
        public virtual int MinimumOrderQuantity { get; set; }
    }
}
