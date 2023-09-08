using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Commerce")]
    public class ProductService : IProductService {
        private readonly IEnumerable<IProductValidityProvider> _validityProviders;

        public ProductService(IEnumerable<IProductValidityProvider> validityProviders) {
            _validityProviders = validityProviders;
        }

        public bool MayAddToCart(ProductPart product) {
            return MayAddToCart(product, 0);
        }

        public bool MayAddToCart(ProductPart product, int quantity) {
            if (product == null) {
                return false;
            }

            // Every validity provider has to confirm that current product can be added to cart.
            // This to avoid a product that is not purchasable for some condition (e.g. no product combination in stock) is added to cart anyway.
            foreach (var provider in _validityProviders) {
                if (!provider.MayAddToCart(product, quantity)) {
                    return false;
                }
            }

            return true;
        }

    }
}
