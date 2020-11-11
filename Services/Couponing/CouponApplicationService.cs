using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public class CouponApplicationService : 
        ICouponApplicationService{

        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CouponApplicationService(
            ICouponRepositoryService couponRepositoryService,
            IShoppingCart shoppingCart,
            IWorkContextAccessor workContextAccessor) {
            _couponRepositoryService = couponRepositoryService;
            _shoppingCart = shoppingCart;
            _workContextAccessor = workContextAccessor;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
        }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;


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
            // implementation of ICartPriceAlterationProcessor for coupons.
            // It will also be used by CouponCartExtensionProvider
            // to write to the user that the coupon is "active".
            var allAlterations = new List<CartPriceAlteration> {
                new CartPriceAlteration {
                    AlterationType = CouponingUtilities.CouponAlterationType,
                    Key = coupon.Code,
                    Weight = 1
                } };
            if (_shoppingCart.PriceAlterations != null) {
                allAlterations.AddRange(_shoppingCart.PriceAlterations);
            }
            _shoppingCart.PriceAlterations = allAlterations.OrderByDescending(cpa => cpa.Weight).ToList();
        }

        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }

        private bool Applies(CouponRecord coupon) {
            //TODO: use criteria to actually check whether the coupon can be used
            if (_shoppingCart?.PriceAlterations != null) {
                // is this coupon already applied?
                if (_shoppingCart.PriceAlterations.Any(cpa => 
                    CouponingUtilities.CouponAlterationType.Equals(cpa.AlterationType)
                    && coupon.Code.Equals(cpa.Key, StringComparison.InvariantCultureIgnoreCase))) {
                    // can't apply the same coupon twice
                    return false;
                }
            }
            return coupon.Published;
        }

        
    }
}
