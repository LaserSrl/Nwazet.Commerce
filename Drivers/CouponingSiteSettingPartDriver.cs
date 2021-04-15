using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingSiteSettingPartDriver : ContentPartDriver<CouponingSiteSettingPart> {

        private readonly IEnumerable<ICouponApplicabilityCriterion> _applicabilityCriteria;
        private readonly IEnumerable<ICouponApplicabilityCriterionProvider> _applicabilityCriteriaProviders;
        private readonly IEnumerable<ICouponLineApplicabilityCriterionProvider> _applicabilityLineCriteriaProviders;

        public CouponingSiteSettingPartDriver(
            IEnumerable<ICouponApplicabilityCriterion> applicabilityCriteria,
            IEnumerable<ICouponApplicabilityCriterionProvider> applicabilityCriteriaProviders,
            IEnumerable<ICouponLineApplicabilityCriterionProvider> applicabilityLineCriteriaProviders) {

            _applicabilityCriteria = applicabilityCriteria;
            _applicabilityCriteriaProviders = applicabilityCriteriaProviders;
            _applicabilityLineCriteriaProviders = applicabilityLineCriteriaProviders;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get {
                return "CouponingSiteSettingPart";
            }
        }

        protected override DriverResult Editor(CouponingSiteSettingPart part, dynamic shapeHelper) {
            var vm = new CouponingSiteSettingViewModel();
            foreach (var provider in _applicabilityCriteria) {
                vm.BasicApplicabilityProviders.Add(new BasicApplicabilityConfigurationViewModel {
                    ProviderName = provider.ProviderName,
                    ProviderLabel = provider.ProviderDisplayName.Text,
                    EvaluateCriterion = provider.EvaluateCriterion()
                });
            }
            foreach (var provider in _applicabilityCriteriaProviders) {
                vm.ApplicabilityProviders.Add(new ProviderConfigurationViewModel {
                    ProviderName = provider.ProviderName,
                    ProviderLabel = provider.ProviderDisplayName.Text,
                    AvailableForConfiguration = provider.IsAvailableForConfiguration(),
                    AvailableForProcessing = provider.IsAvailableForProcessing()
                });
            }
            foreach (var provider in _applicabilityLineCriteriaProviders) {
                vm.LineProviders.Add(new ProviderConfigurationViewModel {
                    ProviderName = provider.ProviderName,
                    ProviderLabel = provider.ProviderDisplayName.Text,
                    AvailableForConfiguration = provider.IsAvailableForConfiguration(),
                    AvailableForProcessing = provider.IsAvailableForProcessing()
                });
            }
            return EditorShape(vm, shapeHelper);
        }

        protected override DriverResult Editor(CouponingSiteSettingPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = new CouponingSiteSettingViewModel();
            if (updater is ECommerceSettingsAdminController
                && updater.TryUpdateModel(vm, Prefix, null, null)) {
                // store the new settings
                part.SetBasicApplicabilityConfiguration(vm.BasicApplicabilityProviders);
                part.SetCouponProvidersConfiguration(vm.ApplicabilityProviders);
                part.SetCouponLineProvidersConfiguration(vm.LineProviders);
            }
            return EditorShape(vm, shapeHelper);
        }

        private DriverResult EditorShape(CouponingSiteSettingViewModel vm, dynamic shapeHelper) {
            return ContentShape("SiteSettings_Couponing",
                () => shapeHelper.EditorTemplate(
                    Prefix: Prefix,
                    TemplateName: "SiteSettings/Couponing",
                    Model: vm
                )
            ).OnGroup("ECommerceSiteSettings");
        }
    }
}
