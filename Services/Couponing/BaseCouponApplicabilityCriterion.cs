using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public abstract class BaseCouponApplicabilityCriterion : ICouponApplicabilityCriterion {
        protected readonly IWorkContextAccessor _workContextAccessor;
        protected readonly ICacheManager _cacheManager;
        protected readonly ISignals _signals;

        public BaseCouponApplicabilityCriterion(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) {

            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public abstract string ProviderName { get; }
        public abstract LocalizedString ProviderDisplayName { get; }

        public virtual bool EvaluateCriterion() {
            var settingForProvider = SettingForProvider();
            if (settingForProvider != null) {
                return settingForProvider.EvaluateCriterion;
            }
            // default true, because implementations represent basic tests that
            // we wanto to be running more often than not.
            return true;
        }

        // empty implementations to override
        public virtual void CanBeAdded(CouponApplicabilityContext context) { }
        public virtual void CanBeProcessed(CouponApplicabilityContext context) { }

        protected BasicApplicabilityConfigurationViewModel SettingForProvider() {
            var settings = GetSettings();
            var settingForProvider = settings.BasicApplicabilityProviders
                .FirstOrDefault(pcvm => ProviderName.Equals(pcvm.ProviderName));
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
                    vm.BasicApplicabilityProviders = settingsPart.BasicApplicabilityProviders.ToList();
                    vm.LineProviders = settingsPart.LineProviders.ToList();
                    return vm;
                });
        }
    }
}
