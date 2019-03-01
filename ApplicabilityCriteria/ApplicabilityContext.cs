using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria {
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
