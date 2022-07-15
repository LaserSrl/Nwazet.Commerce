using System.Collections.Generic;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.Services {
    public interface IShippingMethod : IContent {
        string Name { get; set; }
        string ShippingCompany { get; set; }
        // Shipping areas are stored as comma-delimited lists
        string IncludedShippingAreas { get; set; }
        string ExcludedShippingAreas { get; set; }

        IEnumerable<ShippingOption> ComputePrice(ShippingOptionComputeContext context);
    }
}
