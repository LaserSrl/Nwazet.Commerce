using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Inventory;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlEditViewModel {
        public InventoryControlEditViewModel(
            InventoryControlPart part, 
            IProductInventoryService productInventoryService) {

            PreventAutomaticDecrease = part.PreventAutomaticDecrease;

            ProductSku = part.As<ProductPart>().Sku;
            ContentId = part.Id;
            InventoryValue = productInventoryService.GetInventory(part.As<ProductPart>());
        }

        public bool PreventAutomaticDecrease { get; set; }

        public string ProductSku { get; set; }
        public int ContentId { get; set; }
        public int InventoryValue { get; set; }
    }
}
