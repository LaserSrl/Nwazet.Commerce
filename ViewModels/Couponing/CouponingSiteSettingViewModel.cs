using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingSiteSettingViewModel {

        public CouponingSiteSettingViewModel() {
            ApplicabilityProviders = new List<ProviderConfigurationViewModel>();
            LineProviders = new List<ProviderConfigurationViewModel>();
        }

        public IList<ProviderConfigurationViewModel> ApplicabilityProviders { get; set; }
        public IList<ProviderConfigurationViewModel> LineProviders { get; set; }
    }

    public class ProviderConfigurationViewModel {

        public string ProviderName { get; set; }
        [JsonIgnore]
        public string ProviderLabel { get; set; }
        public bool AvailableForConfiguration { get; set; }
        public bool AvailableForProcessing { get; set; }
    }
}
