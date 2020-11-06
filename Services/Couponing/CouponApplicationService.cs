using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public class CouponApplicationService : ICouponApplicationService {
        private readonly IShoppingCart _shoppingCart;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CouponApplicationService(
            IShoppingCart shoppingCart,
            IWorkContextAccessor workContextAccessor) {

            _shoppingCart = shoppingCart;
            _workContextAccessor = workContextAccessor;
        }

        public void ApplyCoupon(string code) {
            // given the code, find the coupon
            var coupon = GetCouponFromCode(code);
            if (coupon != null) {
                // given the coupon, check whether it's usable
                // then check whether it applies to the current "transaction"
                if (Applies(coupon)) {
                    Apply(coupon);
                }
            }
        }

        private void Apply(CouponRecord coupon) {
            //TODO
            // based on the coupon, we add a CartPriceAlteration to the shoppingCart
            // this object will be used in computing the total cart price by the 
            // implementation of ICartPriceAlterationProcessor for coupons
            // (this same service). It will also be used by CouponCartExtensionProvider
            // to write to the user that the coupon is "active".
            var allAlterations = new List<CartPriceAlteration> {
                new CartPriceAlteration {
                    AlterationType = "Coupon",
                    Key = coupon.Code,
                    Weight = 1
                } };
            if (_shoppingCart.PriceAlterations != null) {
                allAlterations.AddRange(_shoppingCart.PriceAlterations);
            }
            _shoppingCart.PriceAlterations = allAlterations.OrderByDescending(cpa => cpa.Weight).ToList();
        }

        private CouponRecord GetCouponFromCode(string code) {
            //TODO
            if (code.Equals("NATALE20", StringComparison.InvariantCultureIgnoreCase)) {
                return new CouponRecord {
                    Name = "Buon Natale",
                    Code = "NATALE20",
                    Published = true,
                    Value = 10m,
                    CouponType = CouponType.Percent
                };
            }
            return null;
        }
        
        private bool Applies(CouponRecord coupon) {
            //TODO
            return true;
        }
    }
}
