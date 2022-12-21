using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlPart : ContentPart<InventoryControlPartRecord> {
        // This part is responsible for the centralization of the control of inventory quantities.
        // It enables increasing/decreasing them by fixed quantities, and prevents setting them
        // to a fixed value (except during a product's creation).

        public bool PreventAutomaticDecrease {
            get { return Retrieve(r => r.PreventAutomaticDecrease); }
            set { Store(r => r.PreventAutomaticDecrease, value); }
        }
    }
}
