using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.BaseShippingImplementations")]
    public class WeightBasedShippingMethodPartHandler : ContentHandler {
        public WeightBasedShippingMethodPartHandler(IRepository<WeightBasedShippingMethodPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
