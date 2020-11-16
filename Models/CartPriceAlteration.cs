using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    public class CartPriceAlteration {
        /// <summary>
        /// will dictate what service will handle this.
        /// Example value: "Coupon"
        /// </summary>
        public string AlterationType { get; set; }
        /// <summary>
        /// Tells the specific alteration.
        /// Example: a coupon's code
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// In case we need to give a specific order to alterations
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// Action to remove this CartPriceAlteration from the cart.
        /// </summary>
        public string RemovalAction { get; set; }
    }
}
