using Nwazet.Commerce.Models;
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
                // doing it like this adds a second Inventory shape, because 
                // DriverPartCoordinator doesn't care and executes all DriverResults
                // it receives: none is overriding any other. I should try from
                // a handler.BuildEditor to affect the placement somehow, so
                // that the "default" inventory shape doesn't get applied.
                shapes.Add(ContentShape("Parts_Inventory_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: part.Id == 0 
                            ? "Parts/Inventory"
                            : "Parts/InventoryControl/Inventory",
                        Model: new InventoryEditViewModel(part.As<InventoryPart>()),
                        Prefix: "InventoryPart")));
            }

            // shape for the PreventAutomaticDecrease flag
            shapes.Add(ContentShape("Parts_InventoryControl_Edit",
                () => {
                    var viewModel = new InventoryControlEditViewModel(part);
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
