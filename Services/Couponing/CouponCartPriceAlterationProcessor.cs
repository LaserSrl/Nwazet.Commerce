using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponCartPriceAlterationProcessor : ICartPriceAlterationProcessor {

        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICouponApplicationService _couponApplicationService;
        private readonly IProductPriceService _productPriceService;
        private readonly IVatConfigurationService _vatConfigurationService;

        public CouponCartPriceAlterationProcessor(
            ICouponRepositoryService couponRepositoryService,
            IWorkContextAccessor workContextAccessor,
            ICouponApplicationService couponApplicationService,
            IProductPriceService productPriceService,
            IVatConfigurationService vatConfigurationService) {

            _couponRepositoryService = couponRepositoryService;
            _workContextAccessor = workContextAccessor;
            _couponApplicationService = couponApplicationService;
            _productPriceService = productPriceService;
            _vatConfigurationService = vatConfigurationService;

            _loadedCoupons = new Dictionary<string, CouponRecord>();

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;


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

        public decimal AlterationAmount(
            CartPriceAlteration alteration, IShoppingCart shoppingCart) {
            // The values returned by this method are "after VAT".
            // should this provider process the given CartPriceAlteration?
            if (CanProcess(alteration, shoppingCart)) {
                // get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                // Do the computation
                switch (coupon.CouponType) {
                    // The total amount for the cart is the result of adding up the amounts 
                    // for each line.
                    case CouponType.Percent:
                    case CouponType.Amount:
                        return shoppingCart.GetProducts()
                            .Sum(cartLine => {
                                var discount = AlterationAmount(alteration, shoppingCart, cartLine);
                                // apply VAT and such
                                return _productPriceService
                                    .GetPrice(cartLine.Product, discount, shoppingCart.Country, shoppingCart.ZipCode);
                            });
                    default:
                        break;
                }
            }
            return 0.0m;
        }

        public decimal AlterationAmount(
            CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine) {
            // The amounts returned by this method are "before VAT"
            if (CanProcess(alteration, shoppingCart, cartLine)) {
                // Coupons on single product lines
                // Get the coupon corresponding to the alteration
                var coupon = GetCouponFromCode(alteration.Key);
                var quantity = cartLine.Quantity; // TODO: max quantity to consider for discount
                switch (coupon.CouponType) {
                    case CouponType.Percent:
                        // Consider price as input, before VAT and such
                        var itemPrice = cartLine.Product.DiscountPrice >= 0 && cartLine.Product.DiscountPrice < cartLine.Product.Price
                            ? cartLine.Product.DiscountPrice //_productPriceService.GetDiscountPrice(cartLine.Product, shoppingCart.Country, shoppingCart.ZipCode)
                            : cartLine.Product.Price; // _productPriceService.GetPrice(cartLine.Product, shoppingCart.Country, shoppingCart.ZipCode);
                        
                        var linePrice = Math.Round(itemPrice * quantity, 2)
                            + cartLine.LinePriceAdjustment;
                        return -linePrice * (coupon.Value / 100m);
                    case CouponType.Amount:
                        // Fixed amount discount for each single item. Compute it here before VAT.
                        // coupon.Value is after VAT. Meaning that if you input 1.1, and the VAT is 10%,
                        // this should return (-quantity * 1)
                        var rate = _vatConfigurationService
                            .GetRate(cartLine.Product, shoppingCart.Country, shoppingCart.ZipCode);
                        var value = coupon.Value / (1m + rate);
                        return -quantity * value;

                    //    // flat coupon on the cart? That does nothing clear
                    //    // to a single product line
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
            return null;
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
                result = _couponApplicationService.CanProcess(context);
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
                result = _couponApplicationService.CanProcess(context);
            }

            return result;
        }
    }
}
