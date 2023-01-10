using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Drivers.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class BaseInventoryPartDriver : ContentPartDriver<InventoryPart> {
        // When this feature is active, it deactivates the default InventoryPart shape
        // that would allow inputing the inventory quantity directly. This driver 
        // restores it for those cases when the ContentItem doesn't have an
        // InventoryControlPart.

        protected override string Prefix => "InventoryPart";

        protected override DriverResult Editor(
            InventoryPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }


        protected override DriverResult Editor(
            InventoryPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (part.Id == 0 || !part.Is<InventoryControlPart>()) {
                // creation of new content
                return ContentShape("Parts_InventoryControl_Quantity_Edit",
                    () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/InventoryControl/Inventory.BaseQuantity",
                    Model: new InventoryEditViewModel(part),
                    Prefix: Prefix));
            }

            return null;
        }
        
    }
}
