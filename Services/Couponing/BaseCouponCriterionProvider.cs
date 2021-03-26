using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Nwazet.Commerce.Services.Couponing {
    public abstract class BaseCouponCriterionProvider : ICouponCriterionProvider {
        protected readonly IWorkContextAccessor _workContextAccessor;
        protected readonly ICacheManager _cacheManager;
        protected readonly ISignals _signals;

        public BaseCouponCriterionProvider(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) {

            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public abstract string ProviderName { get; }
        public abstract LocalizedString ProviderDisplayName { get; }
        
        public virtual bool IsAvailableForConfiguration() {
            var settingForProvider = SettingForProvider();
            if (settingForProvider != null) {
                return settingForProvider.AvailableForConfiguration;
            }
            return false;
        }

        public virtual bool IsAvailableForProcessing() {
            var settingForProvider = SettingForProvider();
            if (settingForProvider != null) {
                return settingForProvider.AvailableForProcessing;
            }
            return false;
        }

        protected ProviderConfigurationViewModel SettingForProvider() {
            var settings = GetSettings();
            var settingForProvider = settings.ApplicabilityProviders
                .FirstOrDefault(pcvm => ProviderName.Equals(pcvm.ProviderName));
            if (settingForProvider == null) {
                settingForProvider = settings.LineProviders
                .FirstOrDefault(pcvm => ProviderName.Equals(pcvm.ProviderName));
            }
            return settingForProvider;
        }

        // settings part should be cached to prevent repeated fetches of the Site ContentItem
        protected CouponingSiteSettingViewModel GetSettings() {
            return _cacheManager.Get(CouponingSiteSettingPart.CacheKey,
                ctx => {
                    ctx.Monitor(_signals.When(CouponingSiteSettingPart.CacheKey));
                    var settingsPart = _workContextAccessor.GetContext()
                        .CurrentSite.As<CouponingSiteSettingPart>();
                    var vm = new CouponingSiteSettingViewModel();
                    vm.ApplicabilityProviders = settingsPart.ApplicabilityProviders.ToList();
                    vm.LineProviders = settingsPart.LineProviders.ToList();
                    return vm;
                });
        }
    }
}
