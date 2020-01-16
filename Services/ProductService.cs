using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Commerce")]
    public class ProductService : IProductService {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizer _authorizer;

        public ProductService(
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizer authorizer
            ) {
            _contentDefinitionManager = contentDefinitionManager;
            _authorizer = authorizer;
        }

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

        public IEnumerable<ContentTypeDefinition> GetProductTypes() {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(pa => pa
                    .PartDefinition.Name.Equals(ProductPart.PartName, StringComparison.InvariantCultureIgnoreCase) &&
                    _authorizer.Authorize(Orchard.Core.Contents.Permissions.CreateContent)));
        }
    }
}
