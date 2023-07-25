using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Inventory {
    public interface IProductGroupInventoryProvider : IDependency {

        IEnumerable<ProductPart> AddProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> previous);

        IEnumerable<ProductPart> FilterProductsWithSameInventory(
            ProductPart part, IEnumerable<ProductPart> products);

        IEnumerable<IEnumerable<ProductPart>> AddProductsWithInventoryIssues();

        IEnumerable<IEnumerable<ProductPart>> FilterProductsWithInventoryIssues(
            IEnumerable<IEnumerable<ProductPart>> groups);
    }
}
