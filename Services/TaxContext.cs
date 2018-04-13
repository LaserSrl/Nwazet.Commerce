using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    /// <summary>
    /// This class is used to wrap the information required for computation of the correct
    /// tax amounts.
    /// </summary>
    public class TaxContext {
        #region Properties required for compatibility with old tax feature
        public IEnumerable<ShoppingCartQuantityProduct> ShoppingCartQuantityProducts { get; set; }
        public decimal CartSubTotal { get; set; }
        public decimal ShippingPrice { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        #endregion

    }
}
