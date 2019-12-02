using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Services {
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
