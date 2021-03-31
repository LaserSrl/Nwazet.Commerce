using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models.Couponing {
    public enum CouponType {
        // Percent of the price of each single item it applies to.
        Percent,
        // Fixed amount off the price of each single item it applies to.
        // If the amount is 1$, and a cartline the coupon applies to has
        // quantity = 3, the coupon will take 3$ off the final (after VAT)
        // price for that line.
        Amount,
        // Fixed amount off the cart total.
        // If the amount is 10$, the coupon will take 10$ off the final
        // (after VAT) price for the whole cart.
        CartAmount
    }
}
