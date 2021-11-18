using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Inventory {
    [OrchardFeature("Nwazet.InventoryBySKU")]
    public class ProductGroupInventoryProviderBySKU : ProductGroupInventoryProviderBase {
        protected readonly IContentManager _contentManager;

        public ProductGroupInventoryProviderBySKU(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor)
            : base(workContextAccessor) {

            _contentManager = contentManager;
        }

        public override IEnumerable<ProductPart> AddProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> previous) {
            // All products that have the same SKU as the product under test
            var sSet = new List<ProductPart>();
            if (part.Record.Sku != null && part.Record.ContentItemRecord != null) {
                sSet.AddRange(_contentManager
                    .Query<ProductPart, ProductPartVersionRecord>(VersionOptions.Latest)
                    .Where(pa => pa.Sku == part.Record.Sku && pa.ContentItemRecord.Id != part.Record.ContentItemRecord.Id)
                    .List());
            }
            return sSet;
        }

        public override IEnumerable<ProductPart> AddProductsWithInventoryIssues() {
            var badProducts = _contentManager
                .Query<ProductPart, ProductPartVersionRecord>(VersionOptions.Latest)
                .List() //Get all ProductParts
                .GroupBy(pp => pp.Sku) //group them based on their SKU
                .Where(group => group.Count() > 1) //single products are not groups
                .Where(group => group
                    .Select(pp => GetInventory(pp))
                    .Distinct()
                    .Count() > 1) //groups where the inventories are not all the same
                .Select(group => group.First()); //get the first ProductPart as representative of the group

            return badProducts;
        }

    }
}
