using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class ApplicabilityContext {

        public ApplicabilityContext(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities,
            IEnumerable<IShippingMethod> shippingMethods,
            string country,
            string zipCode) {

            ProductQuantities = productQuantities;
            ShippingMethods = shippingMethods;
            Country = country;
            ZipCode = zipCode;
        }

        public IEnumerable<ShoppingCartQuantityProduct> ProductQuantities { get; set; }
        public IEnumerable<IShippingMethod> ShippingMethods { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
    }
}
