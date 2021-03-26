using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public class CartFinalizedContext {
        /// <summary>
        /// The OrderPart corresponding to the order that caused the cart to be finalized.
        /// </summary>
        public OrderPart Order { get; set; }
    }
}
