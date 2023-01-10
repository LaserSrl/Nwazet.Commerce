using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlContentHandler : ContentHandler {

        public InventoryControlContentHandler(
            IRepository<InventoryControlPartRecord> inventoryControlPartRepository) {

            Filters.Add(StorageFilter.For(inventoryControlPartRepository));
        }
    }
}
