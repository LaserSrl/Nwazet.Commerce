using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Services {
    // This is the standard product validity provider
    // It replies the checks previously applied inside Nwazet.Commerce.Services.ProductService
    public class ProductValidityProvider : IProductValidityProvider {
        public bool MayAddToCart(ProductPart part, int quantity) {
            if (part == null) {
                return false;
            }

            return (part.Inventory > 0 && part.Inventory >= quantity) || part.AllowBackOrder
                || (part.IsDigital && !part.ConsiderInventory);
        }
    }
}
