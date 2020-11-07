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
            yield return _shapeFactory.CouponingCartExtension();
            //TODO: write on frontend a note for the customer congratulating them for 
            // having active coupons, if any
        }
    }
}
