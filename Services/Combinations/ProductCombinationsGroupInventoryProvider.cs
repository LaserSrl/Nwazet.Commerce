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
            var results = new List<IEnumerable<ProductPart>>();
            foreach (var group in productGroups) {
                // If a group consists exclusively of a container and some of its
                // combinations, or just combinations of the same container, it's
                // likely not really an issue that their inventories are different.

                // If any product is neither a CombinationContainerPart nor a 
                // CombinationPart, we cannot say the group doesn't have inventory
                // issues.
                if (group.Any(p => !p.Is<CombinationContainerPart>() && !p.Is<CombinationPart>())) {
                    continue;
                }
                var containers = group.Select(p => p.As<CombinationContainerPart>())
                    .Where(c => c != null);
                // If there is more than one container, we definitely aren't in a 
                // condition we can discard.
                if (containers.Count() > 1) {
                    continue;
                }
                var combinations = group.Select(p => p.As<CombinationPart>())
                    .Where(c => c != null);
                // Here we have at most 1 container, and all combinations.
                // Make sure the combinations are all for the same product.
                var containerIds = combinations.Select(c => c.CombinationContainerPart.Id).Distinct();
                // If they belong to different containers, we cannot say the group
                // doesn't have inventory issues.
                if (containerIds.Count() > 1) {
                    continue;
                }
                var containerId = containerIds.FirstOrDefault();
                // If there actually is a Container from the steps above, and it's
                // not the same as the one for the combinations, we cannot say the
                // group doesn't have inventory issues.
                var container = containers.FirstOrDefault();
                if (container != null && container.Id != containerId) {
                    continue;
                }
                // Here:
                // The group is all combinations from the same container, and it may
                // include the container itself, so it's fine if the inventories are
                // not all equal.
                results.Add(group);
            }
            
            return results.ToList(); // Enumerate just in case.
        }
    }
}
