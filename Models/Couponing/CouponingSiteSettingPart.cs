using Newtonsoft.Json;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingSiteSettingPart : ContentPart {
        public const string CacheKey = "CouponingSiteSettingPart";

        public virtual string CouponBasicApplicabilityConfiguration {
            get { return this.Retrieve(p => p.CouponBasicApplicabilityConfiguration); }
            set { this.Store(p => p.CouponBasicApplicabilityConfiguration, value); }
        }

        public virtual string CouponProvidersConfiguration {
            get { return this.Retrieve(p => p.CouponProvidersConfiguration); }
            set { this.Store(p => p.CouponProvidersConfiguration, value); }
        }

        public virtual string CouponLineProvidersConfiguration {
            get { return this.Retrieve(p => p.CouponLineProvidersConfiguration); }
            set { this.Store(p => p.CouponLineProvidersConfiguration, value); }
        }
        public virtual bool CouponFormVisibility {
            get { return this.Retrieve(p => p.CouponFormVisibility); }
            set { this.Store(p => p.CouponFormVisibility, value); }
        }

        public IList<BasicApplicabilityConfigurationViewModel> BasicApplicabilityProviders {
            get {
                try {
                    return JsonConvert
                        .DeserializeObject<List<BasicApplicabilityConfigurationViewModel>>(
                            CouponBasicApplicabilityConfiguration ?? "[]");
                } catch (Exception) {
                    return new List<BasicApplicabilityConfigurationViewModel>();
                }
            }
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

        public void SetBasicApplicabilityConfiguration(IList<BasicApplicabilityConfigurationViewModel> providers) {
            CouponBasicApplicabilityConfiguration = JsonConvert.SerializeObject(providers);
        }

        public void SetCouponProvidersConfiguration(IList<ProviderConfigurationViewModel> providers) {
            CouponProvidersConfiguration = JsonConvert.SerializeObject(providers);
        }

        public void SetCouponLineProvidersConfiguration(IList<ProviderConfigurationViewModel> providers) {
            CouponLineProvidersConfiguration = JsonConvert.SerializeObject(providers);
        }
    }
}
