using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingSiteSettingDriver : ContentPartDriver<FlexibleShippingSiteSettingPart> {
        public FlexibleShippingSiteSettingDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get {
                return "FlexibleShippingSiteSettingPart";
            }
        }

        protected override DriverResult Editor(FlexibleShippingSiteSettingPart part, dynamic shapeHelper) {
            var vm = new FlexibleShippingSiteSettingViewModel();
            vm.EnablePriceTiers = part.EnablePriceTiers;

            return EditorShape(vm, shapeHelper);
        }

        protected override DriverResult Editor(FlexibleShippingSiteSettingPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = new FlexibleShippingSiteSettingViewModel();
            if (updater is ECommerceSettingsAdminController
                && updater.TryUpdateModel(vm, Prefix, null, null)) {
                part.EnablePriceTiers = vm.EnablePriceTiers;
            }
            return EditorShape(vm, shapeHelper);
        }

        private DriverResult EditorShape(FlexibleShippingSiteSettingViewModel vm, dynamic shapeHelper) {
            return ContentShape("SiteSettings_FlexibleShipping",
                () => shapeHelper.EditorTemplate(
                    Prefix: Prefix,
                    TemplateName: "SiteSettings/FlexibleShipping",
                    Model: vm
                )
            ).OnGroup("ECommerceSiteSettings");
        }
    }
}
