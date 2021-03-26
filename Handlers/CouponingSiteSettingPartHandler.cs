using Nwazet.Commerce.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingSiteSettingPartHandler : ContentHandler {
        private readonly ISignals _signals;

        public CouponingSiteSettingPartHandler(
            ISignals signals) {
            Filters.Add(new ActivatingFilter<CouponingSiteSettingPart>("Site"));

            _signals = signals;


            // Evict cached content when updated, removed or destroyed.
            OnUpdated<CouponingSiteSettingPart>(
                (context, part) => Invalidate());
            OnImported<CouponingSiteSettingPart>(
                (context, part) => Invalidate());
            OnPublished<CouponingSiteSettingPart>(
                (context, part) => Invalidate());
            OnRemoved<CouponingSiteSettingPart>(
                (context, part) => Invalidate());
            OnDestroyed<CouponingSiteSettingPart>(
                (context, part) => Invalidate());
        }

        private void Invalidate() {
            _signals.Trigger(CouponingSiteSettingPart.CacheKey);
        }
    }
}
