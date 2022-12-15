using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Inventory;
using Nwazet.Commerce.ViewModels;
using Nwazet.Commerce.ViewModels.InventoryControl;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
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
        private readonly IProductInventoryService _productInventoryService;

        public InventoryControlPartDriver(
            IProductInventoryService productInventoryService) {

            _productInventoryService = productInventoryService;
        }

        protected override string Prefix => "InventoryControlPart";

        protected override DriverResult Editor(
            InventoryControlPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(
            InventoryControlPart part, IUpdateModel updater, dynamic shapeHelper) {

            var shapes = new List<DriverResult>();

            // shape for the InventoryPart
            if (part.Is<InventoryPart>()) {
                // We are going to use the ShapeTableCreated to alter the shape tables so that
                // the default shape usually displayed for inventory isn't shown anymore.
                shapes.Add(ContentShape("Parts_InventoryControl_Quantity_Edit",
                    () => {
                        if (part.Id == 0) {
                            // creation of new content
                            return shapeHelper.EditorTemplate(
                                TemplateName: "Parts/InventoryControl/Inventory.BaseQuantity",
                                Model: new InventoryEditViewModel(part.As<InventoryPart>()),
                                Prefix: "InventoryPart");
                        }
                        else {
                            return shapeHelper.EditorTemplate(
                                TemplateName: "Parts/InventoryControl/Inventory.Quantity",
                                Model: new InventoryControlEditViewModel(part, _productInventoryService),
                                Prefix: Prefix);
                        }
                    }));
            }

            // shape for the PreventAutomaticDecrease flag
            shapes.Add(ContentShape("Parts_InventoryControl_Edit",
                () => {
                    var viewModel = new InventoryControlEditViewModel(part, _productInventoryService);
                    if (updater != null) {
                        updater.TryUpdateModel(viewModel, Prefix, null, null);
                        part.PreventAutomaticDecrease = viewModel.PreventAutomaticDecrease;
                    }
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/InventoryControl/InventoryControl",
                        Model: viewModel,
                        Prefix: Prefix);
                }));

            return Combined(shapes.ToArray());
        }

        protected override void Exporting(
            InventoryControlPart part, ExportContentContext context) {

            var el = context.Element(typeof(InventoryPart).Name);
            el.With(part)
                .ToAttr(p => p.PreventAutomaticDecrease);
        }

        protected override void Importing(
            InventoryControlPart part, ImportContentContext context) {

            var el = context.Data.Element(typeof(InventoryControlPart).Name);
            if (el == null) {
                return;
            }
            el.With(part)
                .FromAttr(p => p.PreventAutomaticDecrease);
        }

        protected override void Cloning(
            InventoryControlPart originalPart, InventoryControlPart clonePart, CloneContentContext context) {
            clonePart.PreventAutomaticDecrease = originalPart.PreventAutomaticDecrease;
        }
    }
}
