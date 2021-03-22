using Nwazet.Commerce.Extensions;
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
    public class CouponCartLifeCycleEventHandler : ICartLifeCycleEventHandler {
        private readonly IShoppingCart _shoppingCart;
        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICouponApplicationService _couponApplicationService;

        public CouponCartLifeCycleEventHandler(
            IShoppingCart shoppingCart,
            ICouponRepositoryService couponRepositoryService,
            IWorkContextAccessor workContextAccessor,
            ICouponApplicationService couponApplicationService) {

            _shoppingCart = shoppingCart;
            _couponRepositoryService = couponRepositoryService;
            _workContextAccessor = workContextAccessor;
            _couponApplicationService = couponApplicationService;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
        }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;

        public void Finalized(CartFinalizedContext context) {
            // here _shoppingCart should still have all its stuff inside
            var coupons = CouponsFromCart();
            foreach (var coupon in coupons) {
                var couponContext = new CouponUsedContext {
                    Coupon = coupon,
                    ShoppingCart = _shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                    Order = context.Order
                };
                
                _couponApplicationService.CouponUsed(couponContext);
            }
        }

        public void ItemAdded(ShoppingCartItem item) {
            Updated();
        }

        public void ItemRemoved(ShoppingCartItem item) {
            Updated();
        }

        public void Updated() {
            // TODO: some coupons may ned to be re-evaluated, because depending
            // on how they are defined and what conditions they require they may
            // not be valid anymore for the current cart.
            var coupons = CouponsFromCart();
            foreach (var coupon in coupons) {
                var context = new CouponLifeUpdateContext {
                    Coupon = coupon,
                    ShoppingCart = _shoppingCart,
                    WorkContext = _workContextAccessor.GetContext()
                };
                _couponApplicationService.ReevaluateValidity(context);
            }
        }

        public void Updated(IEnumerable<ShoppingCartItem> addedItems, IEnumerable<ShoppingCartItem> removedItems) {
            Updated();
        }

        private IEnumerable<CouponRecord> CouponsFromCart() {
            var couponCodes = _shoppingCart.PriceAlterations
                .Where(cpa => 
                    // only alterations for coupons
                    CouponingUtilities.CouponAlterationType
                        .Equals(cpa.AlterationType, StringComparison.InvariantCultureIgnoreCase))
                .Select(cpa => cpa.Key);

            return couponCodes
                .Select(cc => GetCouponFromCode(cc))
                // Should we consider only those coupons that can actually be processed,
                // meaning they would actually affect the operation/cart in any way?
                ;
        }

        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }
    }
}
