using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicationService : 
        ICouponApplicationService {

        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INotifier _notifier;
        private readonly IEnumerable<ICouponApplicabilityCriterion> _applicabilityCriteria;

        public CouponApplicationService(
            ICouponRepositoryService couponRepositoryService,
            IShoppingCart shoppingCart,
            IWorkContextAccessor workContextAccessor,
            INotifier notifier,
            IEnumerable<ICouponApplicabilityCriterion> applicabilityCriteria) {

            _couponRepositoryService = couponRepositoryService;
            _shoppingCart = shoppingCart;
            _workContextAccessor = workContextAccessor;
            _notifier = notifier;
            _applicabilityCriteria = applicabilityCriteria;

            _loadedCoupons = new Dictionary<string, CouponRecord>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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
                    _notifier.Information(T("Coupon {0} was successfully applied", coupon.Code));
                }
            } else {
                _notifier.Warning(T("Coupon code {0} is not valid", code));
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
                    Weight = 1,
                    RemovalAction = GetRemoveActionUrl()
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
            var context = new CouponApplicabilityContext {
                Coupon = coupon,
                ShoppingCart = _shoppingCart,
                WorkContext = _workContextAccessor.GetContext(),
                IsApplicable = coupon.Published
            };
            foreach (var criterion in _applicabilityCriteria) {
                criterion.CanBeAdded(context);
            }
            var result = context.IsApplicable;
            if (!result) {
                if (context.Message != null && !string.IsNullOrWhiteSpace(context.Message.Text)) {
                    _notifier.Warning(context.Message);
                } else {
                    _notifier.Warning(T("Coupon code {0} is not valid", coupon.Code));
                }
            }
            return result;
        }

        private string GetRemoveActionUrl() {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Remove", "Coupon", new { area = "Nwazet.Commerce" });
        }

        public void RemoveCoupon(string code) {
            if (!string.IsNullOrWhiteSpace(code)) {
                if (_shoppingCart.PriceAlterations
                    .Any(cap =>
                        CouponingUtilities.CouponAlterationType.Equals(cap.AlterationType, StringComparison.InvariantCultureIgnoreCase)
                        && code.Equals(cap.Key, StringComparison.InvariantCultureIgnoreCase))) {
                    // we do that if before actually attempting to remove just so we can give a notification
                    // otherwise we may end up giving it even when we are not removing anything
                    _shoppingCart.PriceAlterations = _shoppingCart.PriceAlterations
                        .Where(cap =>
                            // Do not remove alterations that are not coupons
                            !CouponingUtilities.CouponAlterationType.Equals(cap.AlterationType, StringComparison.InvariantCultureIgnoreCase)
                            // Use the give key to remove a coupon (if it exists)
                            || !code.Equals(cap.Key, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                    _notifier.Information(T("Coupon {0} was removed", code));
                }
            }
        }
        

        public void ReevaluateValidity(CouponLifeUpdateContext context) {
            //TODO
        }

        public void CouponUsed(CouponLifeUpdateContext context) {
            //TODO
            // Maybe it would make sense to fire off coupon-related events?
        }
    }
}
