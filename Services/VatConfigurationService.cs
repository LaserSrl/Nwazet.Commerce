using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
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

        public int GetDefaultCategoryId() {
            _contentManager.Query<VatConfigurationPart, VatConfigurationPartRecord>();
            throw new NotImplementedException();
        }
    }
}
