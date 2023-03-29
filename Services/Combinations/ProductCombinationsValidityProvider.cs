using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ProductCombinationsValidityProvider : IProductValidityProvider {
        public bool MayAddToCart(ProductPart part, int quantity) {
            // If there is no CombinationContainerPart, the product can be added to cart (ignore / skip this provider)
            var containerPart = part.As<CombinationContainerPart>();
            if (containerPart == null) {
                return true;
            }

            // If there is no combination, the product can be added to cart (ignore / skip this provider)
            var combinations = containerPart.CombinationParts
                .Where(cp => cp.IsPublished());
            if (!combinations.Any()) {
                return true;
            }

            foreach (var combination in combinations) {
                // At least one combination must be valid to be added to cart.
                // For this reason, function can return at the first valid combination found.
                if (MayAddToCart(combination, quantity)) {
                    return true;
                }
            }

            // If code gets here, it means that there is a CombinationContainerPart with at least a CombinationPart, but no CombinationPart can be added to cart.
            return false;
        }

        private bool MayAddToCart(CombinationPart combination, int quantity) {
            var productPart = combination.As<ProductPart>();
            if (productPart == null) {
                // TODO: is this right?
                // Combinations without a ProductPart shouldn't be added to the cart anyway, 
                // because they can't work as "independent" products.
                return false;
            }
            return (productPart.Inventory > 0 && productPart.Inventory >= quantity) || productPart.AllowBackOrder
                || (productPart.IsDigital && !productPart.ConsiderInventory);
        }
    }
}
