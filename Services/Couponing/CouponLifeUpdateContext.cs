using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponLifeUpdateContext {
        public CouponRecord Coupon { get; set; }
        public IShoppingCart ShoppingCart { get; set; }
        public WorkContext WorkContext { get; set; }
    }
}
