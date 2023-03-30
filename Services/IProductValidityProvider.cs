using Nwazet.Commerce.Models;
using Orchard;

namespace Nwazet.Commerce.Services {
    public interface IProductValidityProvider : IDependency {
        /// <summary>
        /// Checks if the product can be added to the cart, based on specific requirements depending by the provider.
        /// e.g.: the product has a valid inventory, 
        /// there is at least a published product combination with a valid inventory,
        /// the user has a specific role, 
        /// etc.
        /// </summary>
        /// <param name="part">The product to be added to cart</param>
        /// <param name="quantity">The quantity of the product to be added to cart</param>
        /// <returns></returns>
        bool MayAddToCart(ProductPart part, int quantity);
    }
}
