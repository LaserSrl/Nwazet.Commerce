using System.Collections.Generic;
using Nwazet.Commerce.Models;
using Orchard;

namespace Nwazet.Commerce.Services {
    public interface ICheckoutService : IDependency {
        string Name { get; }

        dynamic BuildCheckoutButtonShape(
            IEnumerable<dynamic> productShapes, 
            IEnumerable<ShoppingCartQuantityProduct> productQuantities,
            IEnumerable<ShippingOption> shippingOptions,
            TaxAmount taxes,
            string country,
            string zipCode,
            IEnumerable<string> custom);

        string GetChargeAdminUrl(string transactionId);

        /// <summary>
        /// Based on a transaction Id, return some text to be displayed in the 
        /// backoffice. The most common use of this would be to return the name
        /// of the payment system/provider used.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        string GetChargeInfo(string transactionId);
    }
}
