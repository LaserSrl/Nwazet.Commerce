using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers {
    public class InventoryPartDriver : ContentPartDriver<InventoryPart>{

        public InventoryPartDriver() {

        }

        protected override string Prefix => "InventoryPart";

        protected override DriverResult Display(InventoryPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Inventory",
                () => shapeHelper.Parts_Inventory(
                    Inventory: part.ProductInventoryService.GetInventory(part),
                    OutOfStockMessage: part.OutOfStockMessage,
                    AllowBackOrder: part.AllowBackOrder,
                    MinimumOrderQuantity: part.MinimumOrderQuantity));
        }

        protected override DriverResult Editor(InventoryPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Inventory_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Inventory",
                    Model: new InventoryEditViewModel(part),
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(InventoryPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                var viewModel = new InventoryEditViewModel();
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    part.Inventory = viewModel.Inventory;
                    part.OutOfStockMessage = viewModel.OutOfStockMessage;
                    part.AllowBackOrder = viewModel.AllowBackOrder;
                    part.MinimumOrderQuantity = viewModel.MinimumOrderQuantity;
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(InventoryPart part, ExportContentContext context) {
            base.Exporting(part, context);
        }

        protected override void Importing(InventoryPart part, ImportContentContext context) {
            base.Importing(part, context);
        }
    }
}
