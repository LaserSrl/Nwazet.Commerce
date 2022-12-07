using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlEditViewModel {
        public InventoryControlEditViewModel(InventoryControlPart part) {

            PreventAutomaticDecrease = part.PreventAutomaticDecrease;
        }

        public bool PreventAutomaticDecrease { get; set; }
    }
}
