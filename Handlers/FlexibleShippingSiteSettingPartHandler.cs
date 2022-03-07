using Nwazet.Commerce.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingSiteSettingPartHandler : ContentHandler {
        private readonly ISignals _signals;

        public FlexibleShippingSiteSettingPartHandler(ISignals signals) {
            Filters.Add(new ActivatingFilter<FlexibleShippingSiteSettingPart>("Site"));
            _signals = signals;

            // Evict cached content when updated, removed or destroyed.
            OnUpdated<FlexibleShippingMethodPart>(
                (context, part) => Invalidate());
            OnImported<FlexibleShippingMethodPart>(
                (context, part) => Invalidate());
            OnPublished<FlexibleShippingMethodPart>(
                (context, part) => Invalidate());
            OnRemoved<FlexibleShippingMethodPart>(
                (context, part) => Invalidate());
            OnDestroyed<FlexibleShippingMethodPart>(
                (context, part) => Invalidate());
        }

        private void Invalidate() {
            _signals.Trigger(FlexibleShippingSiteSettingPart.CacheKey);
        }
    }
}
