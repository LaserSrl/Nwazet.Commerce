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
            // We also need to ensure that we don't reassign the value of that
            // if it's already populated, to prevent changing it. This will
            // become relevant when new providers are added to compute it and
            // in import/export scenarios.
            OnCreated<OrderPart>((ctx, op) => 
                op.OrderKey = string.IsNullOrWhiteSpace(op.OrderKey)
                    ? op.Id.ToString()
                    : op.OrderKey);
        }

    }
}
