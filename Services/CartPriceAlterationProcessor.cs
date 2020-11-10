using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.Services.Couponing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public class CartPriceAlterationProcessor : ICartPriceAlterationProcessor {


        private readonly ICouponRepositoryService _couponRepositoryService;

        public CartPriceAlterationProcessor(
            ICouponRepositoryService couponRepositoryService) {
            _couponRepositoryService = couponRepositoryService;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
        }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;


        public string AlterationType => "Coupon";

        public bool CanProcess(
            CartPriceAlteration alteration, IShoppingCart shoppingCart) {
            if (alteration.AlterationType == AlterationType) {
                var coupon = GetCouponFromCode(alteration.Key);
                return Applies(coupon, shoppingCart);
            }
            return false;
        }

        public decimal EvaluateAlteration(
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
                    default:
                        return -coupon.Value;
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
