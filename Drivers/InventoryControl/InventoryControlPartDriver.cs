using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlPartDriver 
        : ContentPartCloningDriver<InventoryControlPart> {
    }
}
