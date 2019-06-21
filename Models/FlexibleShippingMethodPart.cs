using Nwazet.Commerce.ApplicabilityCriteria;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPart
        : ContentPart<FlexibleShippingMethodRecord>, IShippingMethod {
        public string Name {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShippingCompany {
            get { return Retrieve(r => r.ShippingCompany); }
            set { Store(r => r.ShippingCompany, value); }
        }
        public string IncludedShippingAreas {
            get { return Retrieve(r => r.IncludedShippingAreas); }
            set { Store(r => r.IncludedShippingAreas, value); }
        }
        public string ExcludedShippingAreas {
            get { return Retrieve(r => r.ExcludedShippingAreas); }
            set { Store(r => r.ExcludedShippingAreas, value); }
        }
        /// <summary>
        /// In cases where VAT or similar taxes are applied on shipping, this is the
        /// price before tax.
        /// </summary>
        public decimal DefaultPrice {
            get { return Retrieve(r => r.DefaultPrice); }
            set { Store(r => r.DefaultPrice, value); }
        }

        public IList<ApplicabilityCriterionRecord> ApplicabilityCriteria {
            get { return Record.ApplicabilityCriteria; }
        }

        public IEnumerable<ShippingOption> ComputePrice(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities,
            IEnumerable<IShippingMethod> shippingMethods,
            string country,
            string zipCode,
            IWorkContextAccessor workContextAccessor) {

            var workContext = workContextAccessor.GetContext();
            IFlexibleShippingManager flexibleShippingManager;
            if (workContext != null
                && workContext.TryResolve(out flexibleShippingManager)) {
                // we have a usable IFlexibleShippingManager here
                if (flexibleShippingManager.TestCriteria(
                    Id, new ApplicabilityContext(
                        productQuantities,
                        shippingMethods,
                        country,
                        zipCode
                    ))) {
                    var price = DefaultPrice;
                    // TODO: make price the result of something?
                    var baseVatConfig = this.As<ProductVatConfigurationPart>();
                    IVatConfigurationService vatConfigurationService;
                    if (baseVatConfig != null
                        && workContext.TryResolve(out vatConfigurationService)) {
                        // a ProductVatConfigurationPart has been attached to this part that is
                        // used to configure shipping. This tells us 2 things:
                        // 1. The AdvancedVAT feature is active.
                        // 2. We want to configure VAT for this shipping.
                        // Using this part for this here is kind of an hack. We really would like to have
                        // a  more coherent system in place.
                        // TODO: fix this hack without breaking anything.
                        var vatConfig = baseVatConfig.VatConfigurationPart
                            ?? vatConfigurationService.GetDefaultCategory();
                        var rate = vatConfigurationService.GetRate(vatConfig);
                        price = price * (1.0m + rate);
                    }
                    yield return GetOption(price);
                }
            }

            yield break;
        }

        private ShippingOption GetOption(decimal price) {
            return new ShippingOption {
                Description = Name,
                Price = price,
                ShippingCompany = ShippingCompany,
                IncludedShippingAreas =
                    IncludedShippingAreas == null
                        ? new string[] { }
                        : IncludedShippingAreas.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                ExcludedShippingAreas =
                    ExcludedShippingAreas == null
                        ? new string[] { }
                        : ExcludedShippingAreas.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                ShippingMethodId = this.Id,
                DefaultPrice = DefaultPrice
            };
        }

        private ShippingOption GetOption() {
            return GetOption(DefaultPrice);
        }
    }
}
