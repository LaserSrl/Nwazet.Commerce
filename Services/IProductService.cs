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
        bool MayAddToCart(ProductPart product);
    }
}
