using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
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
