using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
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
        private readonly IWorkContextAccessor _workContextAccessor;


        public CouponCartExtensionProvider(
            IShoppingCart shoppingCart,
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor) {

            _shoppingCart = shoppingCart;
            _shapeFactory = shapeFactory;
            _workContextAccessor = workContextAccessor;
        }

        public IEnumerable<dynamic> CartExtensionShapes() {

            var couponFormVisibility = _workContextAccessor.GetContext().CurrentSite
                .As<CouponingSiteSettingPart>().CouponFormVisibility;

            if (!couponFormVisibility) {
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
}
