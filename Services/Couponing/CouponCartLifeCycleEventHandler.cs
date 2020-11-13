using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponCartLifeCycleEventHandler : ICartLifeCycleEventHandler {
        public void Finalized() {
            // TODO: "burn" the coupons for the user
        }

        public void ItemAdded(ShoppingCartItem item) { }

        public void ItemRemoved(ShoppingCartItem item) { }

        public void Updated() { }

        public void Updated(IEnumerable<ShoppingCartItem> addedItems, IEnumerable<ShoppingCartItem> removedItems) { }
    }
}
