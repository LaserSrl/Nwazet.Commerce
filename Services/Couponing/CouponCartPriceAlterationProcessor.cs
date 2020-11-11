using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.Services.Couponing;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponCartPriceAlterationProcessor : ICartPriceAlterationProcessor {

        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IProductPriceService _productPriceService;

        public CouponCartPriceAlterationProcessor(
            ICouponRepositoryService couponRepositoryService,
            IProductPriceService productPriceService) {

            _couponRepositoryService = couponRepositoryService;
            _productPriceService = productPriceService;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
        }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;


        public string AlterationType => CouponingUtilities.CouponAlterationType;

        public bool CanProcess(
            CartPriceAlteration alteration, IShoppingCart shoppingCart) {
            if (alteration.AlterationType == AlterationType) {
                var coupon = GetCouponFromCode(alteration.Key);
                return Applies(coupon, shoppingCart);
            }
            return false;
        }

        public decimal AlterationAmount(
            CartPriceAlteration alteration, IShoppingCart shoppingCart) {
            // should this provider process the given CartPriceAlteration?
            if (CanProcess(alteration, shoppingCart)) {
                // get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                // TODO: do the computation
                switch (coupon.CouponType) {
                    case CouponType.Percent:
                        return -shoppingCart.Subtotal() * (coupon.Value / 100m);
                    case CouponType.Amount:
                        return -coupon.Value;
                    default:
                        return 0.0m;
                }
            }
            return 0.0m;
        }

        public decimal AlterationAmount(
            CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine) {
            // TODO
            if (CanProcess(alteration, shoppingCart)) {
                // TODO: coupons on single product lines
                // get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                switch (coupon.CouponType) {
                    case CouponType.Percent:
                        var itemPrice = cartLine.Product.DiscountPrice >= 0 && cartLine.Product.DiscountPrice < cartLine.Product.Price
                            ? _productPriceService.GetDiscountPrice(cartLine.Product, shoppingCart.Country, shoppingCart.ZipCode)
                            : _productPriceService.GetPrice(cartLine.Product, shoppingCart.Country, shoppingCart.ZipCode);
                        var linePrice = itemPrice * cartLine.Quantity
                            + cartLine.LinePriceAdjustment;
                        return -linePrice * (coupon.Value / 100m);
                    case CouponType.Amount:
                        // flat coupon on the cart? That does nothing clear
                        // to a single product line
                    default:
                        return 0.0m;
                }
            }

            return 0.0m;
        }

        public string AlterationLabel(
            CartPriceAlteration alteration, IShoppingCart shoppingCart) {
            // should this provider process the given CartPriceAlteration?
            if (CanProcess(alteration, shoppingCart)) {
                // get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                // TODO: do the computation
                return coupon.Name;
            }
            return null;
        }

        public string AlterationLabel(
            CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine) {
            // TODO
            if (CanProcess(alteration, shoppingCart)) {
                // get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                // TODO: do the computation
                return coupon.Name;
            }
            return null;
        }

        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }

        private bool Applies(CouponRecord coupon, IShoppingCart shoppingCart) {
            //TODO: use criteria to actually check whether the coupon can be used
            return coupon.Published;
        }


    }
}
