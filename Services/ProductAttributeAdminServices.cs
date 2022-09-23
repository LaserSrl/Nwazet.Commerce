using System.Collections.Generic;
using System.Linq;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Attributes")]
    public class ProductAttributeAdminServices : IProductAttributeAdminServices {
        private readonly IContentManager _contentManager;

        public ProductAttributeAdminServices(
            IContentManager contentManager) {

            _contentManager = contentManager;
        }

        public IEnumerable<ProductAttributePart> GetAllProductAttributeParts() {
            return _contentManager
                .Query<ProductAttributePart>()
                .Join<TitlePartRecord>()
                .OrderBy(p => p.Title)
                .List();
        }

        public IEnumerable<ProductAttributePart> GetProductAttributeParts(IEnumerable<int> ids) {
            return _contentManager
                .Query<ProductAttributePart, ProductAttributePartRecord>(VersionOptions.Latest)
                // Converting to array as otherwise an exception "Expression argument must be of type ICollection." is thrown.
                .Where(papr => ids.ToArray().Contains(papr.Id))
                .Join<TitlePartRecord>()
                .OrderBy(p => p.Title)
                .List();
        }
    }
}
