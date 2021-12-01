using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class InventoryPartDetailProvider :
        BaseCombinationDetailProvider {
        public InventoryPartDetailProvider(
            IContentDefinitionManager contentDefinitionManager) 
            : base(contentDefinitionManager) {
        }

        public override ContentTypeDefinitionBuilder AlterCombinationDefinition(
            ContentTypeDefinitionBuilder previous,
            string containerTypeName) {

            var containerDefinition = _contentDefinitionManager.GetTypeDefinition(containerTypeName);

            if (containerDefinition != null
                && containerDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "InventoryPart")) {
                // alter the definition
                return previous.WithPart("InventoryPart");
            }
            return previous;
        }

        public override void Synchronize(
            CombinationContainerPart container, CombinationPart combination) {

            var sourcePart = container.As<InventoryPart>();
            var targetPart = combination.As<InventoryPart>();
            if (sourcePart != null && targetPart != null) {
                // Copy properties from the contaoiner to the combination.
                targetPart.Inventory = sourcePart.Inventory;
                targetPart.OutOfStockMessage = sourcePart.OutOfStockMessage;
                targetPart.AllowBackOrder = sourcePart.AllowBackOrder;
                targetPart.MinimumOrderQuantity = sourcePart.MinimumOrderQuantity;
                targetPart.MaximumOrderQuantity = sourcePart.MaximumOrderQuantity;
            }
        }

        public override IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part, dynamic shapeHelper) {

            var inventoryPart = part.As<InventoryPart>();
            if (inventoryPart != null) {
                var details = new List<CombinationDetailShape>();
                // We need to update the minimum and maximum order quantities.
                details.Add(new CombinationDetailShape {
                    RoleKey = "product-inventory-quantities",
                    Shape = shapeHelper.Combinations_ProductInventoryQuantities(
                        ContentItem: inventoryPart.ContentItem,
                        ProductPart: inventoryPart.As<ProductPart>(),
                        InventoryPart: inventoryPart,
                        CombinationPart: part)
                });
                // Do we need to update the out of stock message?
                return details;
            }
            
            return Enumerable.Empty<CombinationDetailShape>();
        }
    }
}
