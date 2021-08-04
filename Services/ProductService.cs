using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using CorePermissions = Orchard.Core.Contents.Permissions;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Commerce")]
    public class ProductService : IProductService {

        public ProductService() {
        }

        public bool MayAddToCart(ProductPart product) {
            return MayAddToCart(product, 0);
        }

        public bool MayAddToCart(ProductPart product, int quantity) {
            if (product == null) {
                return false;
            }
            return (product.Inventory > 0 && product.Inventory >= quantity) || product.AllowBackOrder
                || (product.IsDigital && !product.ConsiderInventory);
        }

    }
}
