using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPartHandler : ContentHandler {

        public FlexibleShippingMethodPartHandler(
            IRepository<FlexibleShippingMethodRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }
    }
}
