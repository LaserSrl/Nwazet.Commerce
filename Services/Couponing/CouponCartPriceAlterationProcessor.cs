using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponCartPriceAlterationProcessor : ICartPriceAlterationProcessor {

        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICouponEvaluationService _couponEvaluationService;

        public CouponCartPriceAlterationProcessor(
            ICouponRepositoryService couponRepositoryService,
            IWorkContextAccessor workContextAccessor,
            ICouponEvaluationService couponEvaluationService) {

            _couponRepositoryService = couponRepositoryService;
            _workContextAccessor = workContextAccessor;
            _couponEvaluationService = couponEvaluationService;

            _loadedCoupons = new Dictionary<string, CouponRecord>();

            T = NullLocalizer.Instance;

            _processorClass = this.GetType().FullName;
        }
        public Localizer T { get; set; }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;

        private string _processorClass;

        public string AlterationType => CouponingUtilities.CouponAlterationType;

        public bool CanProcess(CartPriceAlteration alteration) {
            if (alteration.AlterationType == AlterationType) {
                var coupon = GetCouponFromCode(alteration.Key);
                return coupon != null;
            }
            return false;
        }

        public bool CanProcess(
            CartPriceAlteration alteration, IShoppingCart shoppingCart) {
            if (CanProcess(alteration)) {
                var coupon = GetCouponFromCode(alteration.Key);
                return Applies(coupon, shoppingCart);
            }
            return false;
        }

        public bool CanProcess(
            CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine) {
            if (CanProcess(alteration, shoppingCart)) {
                var coupon = GetCouponFromCode(alteration.Key);
                return Applies(coupon, shoppingCart, cartLine);
            }
            return false;
        }

        public bool CanProcess(CartPriceAlterationContext context) {
            if (CanProcess(context.Alteration)) {
                var coupon = GetCouponFromCode(context.Alteration.Key);
                return Applies(coupon, context.ShoppingCart);
            }
            return false;
        }

        public decimal AlterationAmount(CartPriceAlterationContext context) {
            // The values returned by this method are "after VAT".
            // should this provider process the given CartPriceAlteration?
            if (CanProcess(context)) {
                // process each line, because the computation for the cart may
                // need those partial results
                foreach (var lineContext in context.ContextsForLines().ToList()) {
                    var newLineValue = 0.0m;
                    var effectiveOnLine = _couponEvaluationService.TryCouponLineValue(lineContext, out newLineValue);
                    lineContext.AlterationValues.Add(new AlterationValueInfo {
                        Alteration = lineContext.Alteration,
                        Value = newLineValue,
                        Label = AlterationLabel(lineContext.Alteration, lineContext.ShoppingCart, lineContext.CartLine),
                        Effective = effectiveOnLine,
                        ProcessorClass = _processorClass
                    });
                }
                // process cart. This computation may use the values from the lines
                // computed above
                var newCartValue = 0.0m;
                var effectiveOnCart = _couponEvaluationService.TryCouponCartValue(context, out newCartValue);
                context.AlterationValues.Add(new AlterationValueInfo {
                    Alteration = context.Alteration,
                    Value = newCartValue,
                    Label = AlterationLabel(context.Alteration, context.ShoppingCart),
                    Effective = effectiveOnCart,
                    ProcessorClass = _processorClass
                });
                return newCartValue;
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
                return coupon.Code;
            }

            return T("{0} not valid", alteration.Key).Text;
        }

        public string AlterationLabel(
            CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine) {
            // TODO
            if (CanProcess(alteration, shoppingCart, cartLine)) {
                // get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                // TODO: do the computation
                return coupon.Code;
            }
            return T("{0} not valid", alteration.Key).Text;
        }

        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }

        protected bool Applies(CouponRecord coupon, IShoppingCart shoppingCart) {
           
            // even more, this will tell us whether this service cna process
            // the coupon. In principle, we could have one very specific service for
            // each coupon configuration.
            // for example, one service would handle coupons that work on a % of the 
            // whole cart, another those that have a fixed amount and so on.
            if (coupon == null) {
                return false;
            }

            var result = coupon.Published;
            if (result) {
                var context = new CouponApplicabilityContext {
                    Coupon = coupon,
                    ShoppingCart = shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                    IsApplicable = coupon.Published
                };
                result = _couponEvaluationService.CanProcess(context);
            }
            // if the service is telling us that the coupon is not valid for
            // the current context+cart, we don't remove it, because that would
            // mess things up in the cart storage.

            return result;
        }

        protected bool Applies(
            CouponRecord coupon, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine) {

            if (coupon == null) {
                return false;
            }

            var result = coupon.Published;
            if (result) {
                var context = new CouponLineApplicabilityContext {
                    Coupon = coupon,
                    ShoppingCart = shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                    IsApplicable = coupon.Published,
                    CartLine = cartLine
                };
                result = _couponEvaluationService.CanProcess(context);
            }

            return result;
        }
    }
}
