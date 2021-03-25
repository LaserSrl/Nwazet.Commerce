using Newtonsoft.Json;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingSiteSettingPart : ContentPart {

        public virtual string CouponProvidersConfiguration {
            get { return this.Retrieve(p => p.CouponProvidersConfiguration); }
            set { this.Store(p => p.CouponProvidersConfiguration, value); }
        }

        public virtual string CouponLineProvidersConfiguration {
            get { return this.Retrieve(p => p.CouponLineProvidersConfiguration); }
            set { this.Store(p => p.CouponLineProvidersConfiguration, value); }
        }

        public IList<ProviderConfigurationViewModel> ApplicabilityProviders {
            get {
                try {
                    return JsonConvert
                        .DeserializeObject<List<ProviderConfigurationViewModel>>(
                            CouponProvidersConfiguration ?? "[]");
                } catch (Exception) {
                    return new List<ProviderConfigurationViewModel>();
                }
            }
        }

        public IList<ProviderConfigurationViewModel> LineProviders {
            get {
                try {
                    return JsonConvert
                        .DeserializeObject<List<ProviderConfigurationViewModel>>(
                            CouponLineProvidersConfiguration ?? "[]");
                } catch (Exception) {
                    return new List<ProviderConfigurationViewModel>();
                }
            }
        }

        public void SetCouponProvidersConfiguration(IList<ProviderConfigurationViewModel> providers) {
            CouponProvidersConfiguration = JsonConvert.SerializeObject(providers);
        }

        public void SetCouponLineProvidersConfiguration(IList<ProviderConfigurationViewModel> providers) {
            CouponLineProvidersConfiguration = JsonConvert.SerializeObject(providers);
        }
    }
}
