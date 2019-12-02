using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    public class InventoryPartHandler : ContentHandler {
        private readonly IProductInventoryService _productInventoryService;

        public InventoryPartHandler(
            IRepository<InventoryPartRecord> repository,
            IProductInventoryService productInventoryService) {

            Filters.Add(StorageFilter.For(repository));

            OnActivated<InventoryPart>((ctx, part) => {
                part.ProductInventoryService = _productInventoryService;
            });
        }

    }
}
