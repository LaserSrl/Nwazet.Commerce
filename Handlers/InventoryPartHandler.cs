using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Inventory;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Commerce")]
    public class InventoryPartHandler : ContentHandler {
        private readonly IProductInventoryService _productInventoryService;

        public InventoryPartHandler(
            IRepository<InventoryPartRecord> repository,
            IProductInventoryService productInventoryService) {
            _productInventoryService = productInventoryService;
            Filters.Add(StorageFilter.For(repository));

            OnActivated<InventoryPart>((ctx, part) => {
                part.ProductInventoryService = _productInventoryService;
            });
        }

    }
}
