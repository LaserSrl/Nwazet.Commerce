using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationService : IVatConfigurationService {

        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public VatConfigurationService(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor) {

            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        private VatConfigurationSiteSettingsPart _settings { get; set; }

        private VatConfigurationSiteSettingsPart Settings {
            get {
                if (_settings == null) {
                    _settings = _workContextAccessor.GetContext().CurrentSite.As<VatConfigurationSiteSettingsPart>();
                }
                return _settings;
            }
        }

        // We save the Id of the VatConfigurationPart for the default TaxProductCategory in
        // the site settings. This way it will always be available.
        // If that value is ==0 we know that no VatConfigurationPart has ever been set as default.
        // Deletion of the default VatConfigurationPart will have to be prevented elsewhere.
        // Deletion can be prevented by using a dynamic permission, but only in those cases where
        // the attempt to delete comes through a method that actually checks for those. For
        // example, admin/contents/remove is fine.

        public int GetDefaultCategoryId() {
            return Settings.DefaultVatConfigurationId;
        }

        public void SetDefaultCategory(VatConfigurationPart part) {
            if (part.ContentItem.Id != GetDefaultCategoryId()) {
                // the part is not the default yet
                Settings.DefaultVatConfigurationId = part.ContentItem.Id;
            }
        }

        public VatConfigurationPart GetDefaultCategory() {

            var id = GetDefaultCategoryId();
            if (id > 0) {
                return _contentManager.Get<VatConfigurationPart>(id);
            } else {
                // no default category set
                return null;
            }
        }
    }
}
