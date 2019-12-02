using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Commerce")]
    public class ProductService : IProductService {
        public bool MayAddToCart(ProductPart product) {
            return MayAddToCart(product, 0);
        }

        public bool MayAddToCart(ProductPart product, int quantity) {
            if (product == null) {
                return false;
            }
            return product.Inventory > quantity || product.AllowBackOrder
                || (product.IsDigital && !product.ConsiderInventory);
        }
    }
}
