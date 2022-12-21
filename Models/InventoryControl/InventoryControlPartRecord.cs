using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlPartRecord : ContentPartRecord {

        public virtual bool PreventAutomaticDecrease { get; set; }
    }
}
