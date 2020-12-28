using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponCartExtensionProvider : ICartExtensionProvider {

        private readonly IShoppingCart _shoppingCart;
        private readonly dynamic _shapeFactory;

        public CouponCartExtensionProvider(
            IShoppingCart shoppingCart,
            IShapeFactory shapeFactory) {

            _shoppingCart = shoppingCart;
            _shapeFactory = shapeFactory;
        }

        public IEnumerable<dynamic> CartExtensionShapes() {
            var appliedCoupons = _shoppingCart
                ?.PriceAlterations
                ?.Where(cpa => CouponingUtilities.CouponAlterationType
                    .Equals(cpa.AlterationType, StringComparison.InvariantCultureIgnoreCase))
                ?? Enumerable.Empty<CartPriceAlteration>();
            yield return _shapeFactory.CouponingCartExtension(AppliedCoupons: appliedCoupons);
            //TODO: write on frontend a note for the customer congratulating them for 
            // having active coupons, if any
        }
    }
}
