using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Events.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlShapeTableEventHandler : IShapeTableEventHandler {
        public void ShapeTableCreated(ShapeTable shapeTable) {
            // We should the suppress the "Parts_Inventory_Quantity_Edit" shape from 
            // InventoryPartDriver.
            ShapeDescriptor editorDescriptor;
            if (shapeTable.Descriptors.TryGetValue("Parts_Inventory_Quantity_Edit", out editorDescriptor)) {
                // to make sure that shape isn't displayed, the descriptor's Placement
                // delegate should return empty string or "-" for its location

                shapeTable.Descriptors["Parts_Inventory_Quantity_Edit"].Placement =
                    (ctx) => new PlacementInfo {
                        Location = "-",
                        Source = string.Empty
                    };
            }
        }
    }
}
