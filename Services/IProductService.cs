using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    /// <summary>
    /// Abstract common operations on products
    /// </summary>
    public interface IProductService : IDependency {
        /// <summary>
        /// Verifies that the conditions are right to add the product to the cart.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        bool MayAddToCart(ProductPart product);
        /// <summary>
        /// Verifies that the conditions are right to add the specific amount of product
        /// to the cart.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        bool MayAddToCart(ProductPart product, int quantity);
    }
}
