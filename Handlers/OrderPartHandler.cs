using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Orders")]
    public class OrderPartHandler : ContentHandler {
        public OrderPartHandler(IRepository<OrderPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            // on created of a new order, we set a property for its
            // unique "order number" that will be used as a reference/key
            // among systems interacting with the e-commerce
            // TODO: make a service for this for future more complex
            // implementations
            OnCreated<OrderPart>((ctx, op) => op.OrderKey = op.Id.ToString());
        }

    }
}
