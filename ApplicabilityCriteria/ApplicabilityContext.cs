using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class ApplicabilityContext {

        public ApplicabilityContext (
            ShippingOptionComputeContext originalContext) {

            ProductQuantities = originalContext.ProductQuantities;
            ShippingMethods = originalContext.ShippingMethods;
            Country = originalContext.Country;
            ZipCode = originalContext.PostalCode;

            ShippingContext = originalContext;
        }

        public IEnumerable<ShoppingCartQuantityProduct> ProductQuantities { get; set; }
        public IEnumerable<IShippingMethod> ShippingMethods { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }

        public ShippingOptionComputeContext ShippingContext { get; set; }
    }
}
