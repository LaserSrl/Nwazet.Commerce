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
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;

        public ProductService(
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizer authorizer,
            IContentManager contentManager
            ) {
            _contentDefinitionManager = contentDefinitionManager;
            _authorizer = authorizer;
            _contentManager = contentManager;
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
                // Type has ProductPart
                .Where(ctd => ctd.Parts.Any(ctpd => ctpd
                    .PartDefinition.Name
                    .Equals(ProductPart.PartName, StringComparison.InvariantCultureIgnoreCase)))
                // We can create ContentItems of that type
                .Where(ctd => {
                    var dummyContent = _contentManager.New(ctd.Name);
                    return _authorizer.Authorize(CorePermissions.CreateContent, dummyContent);
                });
        }
    }
}
