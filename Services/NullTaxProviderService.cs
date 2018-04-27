using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Services {
    /// <summary>
    /// Null implementation of service for the sake of dependency injection
    /// </summary>
    [OrchardFeature("Nwazet.Commerce")]
    public class NullTaxProviderService : ITaxProviderService {
        public TaxContext CreateContext(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities, 
            decimal subtotal, 
            decimal shippingCost, 
            string country, 
            string zipCode) {

            return null;
        }

        public IEnumerable<decimal> ItemizedTaxes(ITax tax, TaxContext context) {
            return Enumerable.Empty<decimal>();
        }

        public decimal ShippingTaxes(ITax tax, TaxContext context) {
            return 0;
        }

        public decimal TotalTaxes(ITax tax, TaxContext context) {
            return 0;
        }
    }
}
