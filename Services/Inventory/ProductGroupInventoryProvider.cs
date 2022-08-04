using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Inventory {
    public class ProductGroupInventoryProvider : ProductGroupInventoryProviderBase {

        protected readonly IContentManager _contentManager;

        public ProductGroupInventoryProvider(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor)
            : base(workContextAccessor) {

            _contentManager = contentManager;
        }

        public override IEnumerable<ProductPart> AddProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> previous) {
            // Return Latest and Published versions, unless they coincide or are the same as part
            var sSet = new ProductPart[] {
                _contentManager.Query<ProductPart>(VersionOptions.Published, part.ContentItem.ContentType)
                    .Where<ProductPartVersionRecord>(ppvr => ppvr.ContentItemRecord == part.Record.ContentItemRecord)
                    .Slice(0, 1).FirstOrDefault(),
                _contentManager.Query<ProductPart>(VersionOptions.Latest, part.ContentItem.ContentType)
                    .Where<ProductPartVersionRecord>(ppvr => ppvr.ContentItemRecord == part.Record.ContentItemRecord)
                    .Slice(0, 1).FirstOrDefault()
            };
            return sSet
                .Distinct()
                .Where(lp => lp != null && lp.Record.Id != part.Record.Id);
        }
    }
}
