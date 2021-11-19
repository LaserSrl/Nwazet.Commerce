using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Inventory;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ProductCombinationsGroupInventoryProvider : ProductGroupInventoryProviderBase {
        
        public ProductCombinationsGroupInventoryProvider(
            IWorkContextAccessor workContextAccessor)
            : base(workContextAccessor) {
            
        }

        public override IEnumerable<ProductPart> FilterProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> products) {
            // If the product is a combination container, we should remove its 
            // combinations from the list of products we should immediately sync
            // inventory on.
            var combinationContainer = part.As<CombinationContainerPart>();
            if (combinationContainer != null) {
                return combinationContainer.CombinationParts
                    .Select(cp => cp.As<ProductPart>())
                    .ToList();
            } else {
                // If the product is a combination, we should remove its container
                // and its siblings (combinations of the same container) from the
                // list of products we should immediately sync inventory on.
                var combination = part.As<CombinationPart>();
                if (combination != null) {
                    var toRemove = new List<ProductPart>();
                    toRemove.Add(combination
                        .CombinationContainerPart.As<ProductPart>());
                    toRemove.AddRange(combination
                        .CombinationContainerPart.CombinationParts
                        .Select(cp => cp.As<ProductPart>()));
                    toRemove.RemoveAll(pp => pp.Id == combination.Id);
                    return toRemove.ToList();
                }
            }
            return Enumerable.Empty<ProductPart>();
        }


        public override IEnumerable<IEnumerable<ProductPart>> FilterProductsWithInventoryIssues(
            IEnumerable<IEnumerable<ProductPart>> productGroups) {
            foreach (var group in productGroups) {
                // If a group consists exclusively of a container and some of its
                // combinations, or just combinations of the same container, it's
                // likely not really an issue that their inventories are different.
            }
            // By default, we are not going to remove any group
            return Enumerable.Empty<IEnumerable<ProductPart>>();
        }
    }
}
