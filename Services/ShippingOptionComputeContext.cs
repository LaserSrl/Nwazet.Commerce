using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public class ShippingOptionComputeContext {

        // Items in the cart
        public IEnumerable<ShoppingCartQuantityProduct> ProductQuantities { get; set; }
        // Configured methods
        public IEnumerable<IShippingMethod> ShippingMethods { get; set; }

        // WorkContext Accessor Service, which also lets us resolve other services at need
        public IWorkContextAccessor WorkContextAccessor { get; set; }

        // Basic representation of destination address
        public string Country { get; set; }
        public string PostalCode { get; set; }

        // TODO: extended information?
    }
}
