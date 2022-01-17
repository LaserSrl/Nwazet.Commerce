using Nwazet.Commerce.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Nwazet.Commerce.Handlers {
    class VatConfigurationSiteSettingsHandler : ContentHandler {
        private readonly ISignals _signals;

        public VatConfigurationSiteSettingsHandler(ISignals signals) {
            _signals = signals;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<VatConfigurationSiteSettingsPart>("Site"));

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

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
            {
                return;
            }
            base.GetItemMetadata(context);
        }

        private void Invalidate() {
            _signals.Trigger(VatConfigurationSiteSettingsPart.CacheKey);
        }
    }
}
