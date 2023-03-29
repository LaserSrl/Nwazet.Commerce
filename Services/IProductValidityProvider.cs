using Nwazet.Commerce.Models;
using Orchard;

namespace Nwazet.Commerce.Services {
    public interface IProductValidityProvider : IDependency {
        bool MayAddToCart(ProductPart part, int quantity);
    }
}
