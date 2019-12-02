using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.Commerce")]
    public class InventoryPartDriver : ContentPartDriver<InventoryPart> {

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
            var el = context.Element(typeof(InventoryPart).Name);
            el.With(part)
                .ToAttr(p => p.Inventory)
                .ToAttr(p => p.OutOfStockMessage)
                .ToAttr(p => p.AllowBackOrder)
                .ToAttr(p => p.MinimumOrderQuantity);
        }

        protected override void Importing(InventoryPart part, ImportContentContext context) {
            var el = context.Data.Element(typeof(InventoryPart).Name);
            if (el == null) {
                return;
            }
            el.With(part)
                .FromAttr(p => p.Inventory)
                .FromAttr(p => p.OutOfStockMessage)
                .FromAttr(p => p.AllowBackOrder)
                .FromAttr(p => p.MinimumOrderQuantity);
        }
    }
}
