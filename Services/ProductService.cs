using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Services {
    public class ProductService : IProductService {
        public bool MayAddToCart(ProductPart product) {
            if (product == null) {
                return false;
            }
            return product.Inventory > 0 || product.AllowBackOrder
                || (product.IsDigital && !product.ConsiderInventory);
        }
    }
}
