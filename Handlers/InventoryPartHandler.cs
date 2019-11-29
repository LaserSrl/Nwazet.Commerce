using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    public class InventoryPartHandler : ContentHandler {

        public InventoryPartHandler(
            IRepository<InventoryPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));

        }

    }
}
