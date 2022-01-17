using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Handlers {
    class VatConfigurationSiteSettingsHandler : ContentHandler {
        private readonly ISignals _signals;

        public VatConfigurationSiteSettingsHandler(ISignals signals) {
            _signals = signals;

            // Evict cached content when updated, removed or destroyed.
            OnUpdated<VatConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnImported<VatConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnPublished<VatConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnRemoved<VatConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
            OnDestroyed<VatConfigurationSiteSettingsPart>(
                (context, part) => Invalidate());
        }

        private void Invalidate() {
            _signals.Trigger(VatConfigurationSiteSettingsPart.CacheKey);
        }
    }
}
