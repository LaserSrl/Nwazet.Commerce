using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponEvaluationService : ICouponEvaluationService {

        private readonly INotifier _notifier;
        private readonly IEnumerable<ICouponApplicabilityCriterion> _applicabilityCriteria;
        private readonly ITokenizer _tokenizer;
        private readonly ICouponCriteriaManagementService _couponCriteriaManagementService;
        private readonly IWorkContextAccessor _workContextAccessor;

        private readonly IProductPriceService _productPriceService;
        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly ICouponRepositoryService _couponRepositoryService;

        public CouponEvaluationService(
            INotifier notifier,
            IEnumerable<ICouponApplicabilityCriterion> applicabilityCriteria,
            ITokenizer tokenizer,
            ICouponCriteriaManagementService couponCriteriaManagementService,
            IWorkContextAccessor workContextAccessor,
            IProductPriceService productPriceService,
            IVatConfigurationService vatConfigurationService,
            ICouponRepositoryService couponRepositoryService) {

            _notifier = notifier;
            _applicabilityCriteria = applicabilityCriteria;
            _tokenizer = tokenizer;
            _couponCriteriaManagementService = couponCriteriaManagementService;
            _workContextAccessor = workContextAccessor;

            _productPriceService = productPriceService;
            _vatConfigurationService = vatConfigurationService;
            _couponRepositoryService = couponRepositoryService;

            _notificationsSent = new HashSet<string>();
            _CACDescriptors = new Dictionary<string, CouponApplicabilityCriterionDescriptor>();
            _CLACDescriptors = new Dictionary<string, CouponLineApplicabilityCriterionDescriptor>();
            _loadedCoupons = new Dictionary<string, CouponRecord>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanProcess(CouponApplicabilityContext context) {
            return TestCriteria(context,
                (cacd, ccc) => cacd.ProcessingCriterion(ccc),
                (cac, ctx) => cac.CanBeProcessed(ctx),
                false);
        }

        public bool CanApply(CouponApplicabilityContext context) {
            return TestCriteria(context,
                (cacd, ccc) => cacd.AdditionCriterion(ccc),
                (cac, ctx) => cac.CanBeAdded(ctx),
                true);
        }

        #region Methods to test validity/applicability of coupon
        private bool TestCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponApplicabilityCriterionContext> descriptorsTest,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest,
            bool isAddition) {

            // Some ICouponApplicabilityCriterion will not have a description because
            // they are there by default for all coupons.
            if (context.IsApplicable) {
                BaseTestCriteria(context, defaultTest);
            }
            // After those, we check for the criteria that are configured explicitly
            // for the coupon.
            if (context.IsApplicable) {
                TestApplicabilityCriteria(context, descriptorsTest);
            }
            // Then we need to evaluate all LineCriteria that are configured for the 
            // coupon. Each criterion has to succeed for at least 1 line of the cart
            if (context.IsApplicable) {
                TestLineCriteria(context);
            }
            // then we need to perform any PostProcessing tests. These tests require computing
            // what the results for having the coupons are. In general, these tests may require
            // a coupon to use information about the other coupons.
            if (context.IsApplicable) {
                if (isAddition) {
                    TestPostApplyCriteria(context);
                } else {
                    TestPostProcessingCriteria(context);
                }
            }
            if (!context.IsApplicable && context.ShouldNotify) {
                if (context.Message == null || string.IsNullOrWhiteSpace(context.Message.Text)) {
                    context.Message = T("Coupon code {0} is not valid", context.Coupon.Code);
                }

                Warning(context.Message);
            }
            return context.IsApplicable;
        }

        private void BaseTestCriteria(
            CouponApplicabilityContext context,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest) {

            // Some ICouponApplicabilityCriterion will not have a description because
            // they are there by default for all coupons.
            foreach (var criterion in _applicabilityCriteria) {
                defaultTest(criterion, context);
            }
        }

        private void TestApplicabilityCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponApplicabilityCriterionContext> descriptorsTest) {

            // TODO: prepare tokens
            Dictionary<string, object> tokens = new Dictionary<string, object>();
            // After those, we check for the criteria that are configured explicitly
            // for the coupon.
            if (context.IsApplicable) {
                foreach (var criterion in context.Coupon.ApplicabilityCriteria) {
                    var tokenizedState = _tokenizer.Replace(criterion.State, tokens);
                    var criterionContext = new CouponApplicabilityCriterionContext {
                        IsApplicable = context.IsApplicable,
                        ApplicabilityContext = context,
                        State = FormParametersHelper.ToDynamic(tokenizedState),
                        CouponRecord = context.Coupon
                    };
                    var descriptor = GetCriterion(criterion.Category, criterion.Type);
                    // descriptor should exist and be enabled
                    if (descriptor == null || !descriptor.IsAvailableForProcessing) {
                        continue;
                    }
                    descriptorsTest(descriptor, criterionContext);
                }
            }
        }

        private Dictionary<string, CouponApplicabilityCriterionDescriptor> _CACDescriptors;
        private CouponApplicabilityCriterionDescriptor
            GetCriterion(string category, string type) {

            var key = string.Join("_", category, type);
            if (!_CACDescriptors.ContainsKey(key)) {
                _CACDescriptors.Add(key, _couponCriteriaManagementService
                    .GetCriterion(category, type));
            }

            return _CACDescriptors[key];
        }

        private void TestLineCriteria(
            CouponApplicabilityContext context) {
            // TODO: prepare tokens
            Dictionary<string, object> tokens = new Dictionary<string, object>();
            // We need to test for each line. Note that this method, if the context
            // is defined for a specific line already, returns itself rather than 
            // a list of contexts for every cart line. We force a ToList() there to
            // force enumerating, so we have the actual objects rather than a reference
            // to how to get them, because otherwise the wrong references may be passed
            // around at later steps (basically, the providers would change the 
            // IsApplicable for an object, then a fresh one would be checked of the
            // flag's value).
            var lineContexts = context.ContextsForLines().ToList();
            foreach (var lineApplicabilityContext in lineContexts) {
                foreach (var criterion in context.Coupon.LineCriteria) {
                    var descriptor = GetLineCriterion(criterion.Category, criterion.Type);
                    // descriptor should exist and be enabled
                    if (descriptor == null || !descriptor.IsAvailableForProcessing) {
                        continue;
                    }

                    var tokenizedState = _tokenizer.Replace(criterion.State, tokens);
                    var lineCriterionContext = new CouponLineCriterionContext {
                        IsApplicable = lineApplicabilityContext.IsApplicable,
                        ApplicabilityContext = lineApplicabilityContext,
                        State = FormParametersHelper.ToDynamic(tokenizedState),
                        CouponRecord = lineApplicabilityContext.Coupon
                    };
                    descriptor.Criterion(lineCriterionContext);
                    // break as soon as we know this line is not ok for the coupon
                    if (!lineCriterionContext.IsApplicable) {
                        context.Message = descriptor.FailureMessage(context);
                        // go to test next line
                        break;
                    }
                }
            }
            // If the criterion fails for all lines, break out
            if (!lineContexts.Any(lctx => lctx.IsApplicable)) {
                context.IsApplicable = false;
            }
        }

        private bool TestCouponCriteriaOnLine(
            CouponLineApplicabilityContext lineApplicabilityContext,
            CouponRecord coupon = null,
            Dictionary<string, object> tokens = null) {

            if (coupon == null) {
                coupon = lineApplicabilityContext.Coupon;
            }
            foreach (var criterion in coupon.LineCriteria) {
                var descriptor = GetLineCriterion(criterion.Category, criterion.Type);
                // descriptor should exist and be enabled
                if (descriptor == null || !descriptor.IsAvailableForProcessing) {
                    continue;
                }

                var tokenizedState = _tokenizer.Replace(criterion.State, tokens);
                var lineCriterionContext = new CouponLineCriterionContext {
                    IsApplicable = lineApplicabilityContext.IsApplicable,
                    ApplicabilityContext = lineApplicabilityContext,
                    State = FormParametersHelper.ToDynamic(tokenizedState),
                    CouponRecord = lineApplicabilityContext.Coupon
                };
                descriptor.Criterion(lineCriterionContext);
                // break as soon as we know this line is not ok for the coupon
                if (!lineCriterionContext.IsApplicable) {
                    lineApplicabilityContext.Message = descriptor.FailureMessage(lineApplicabilityContext);
                    // go to test next line
                    break;
                }
            }

            return lineApplicabilityContext.IsApplicable;
        }

        private Dictionary<string, CouponLineApplicabilityCriterionDescriptor> _CLACDescriptors;
        private CouponLineApplicabilityCriterionDescriptor
            GetLineCriterion(string category, string type) {

            var key = string.Join("_", category, type);
            if (!_CLACDescriptors.ContainsKey(key)) {
                _CLACDescriptors.Add(key, _couponCriteriaManagementService
                    .GetLineCriterion(category, type));
            }

            return _CLACDescriptors[key];
        }


        private void TestPostApplyCriteria(
            CouponApplicabilityContext context) {
            // Here we need to be able to pass to each criterion the information
            // about other criteria from all coupons in the cart.
            // Example cases we need to be able to handle:
            // - We are attempting to add coupon 'X' to the cart. The cart already has
            //   a non-cumulative coupon 'Y'. Non of the criteria for 'X' handles the 
            //   fact that 'Y' should prevent addition of other coupons.
            // - We are attempting to add coupon 'X' to the cart. Adding this coupon
            //   would cause the price for a line of the cart to go negative. The site
            //   is configured to prevent line prices from ever going negative.

            // Get all the coupons from the cart
            var coupons = context.ShoppingCart
                .PriceAlterations
                .Where(cpa => cpa.AlterationType == CouponingUtilities.CouponAlterationType)
                .Select(cpa => GetCouponFromCode(cpa.Key))
                .ToList();
            // add the current coupon to that list
            coupons.Add(context.Coupon);
            // make sure that the list is sorted correctly
            coupons = coupons
                .OrderByDescending(c => CouponingUtilities.CouponAlterationWeight(c))
                .ToList();

            var postProcessingContext = new CouponPostApplicabilityContext(context, coupons);
            // for each line, compute the contribution of every coupon, in order
            foreach (var lineContext in postProcessingContext.LineContexts) {
                lineContext.BaseLinePrice = GetLinePrice(lineContext.CartLine);
                var previousLineValues = new List<decimal>();
                foreach (var couponInfo in lineContext.CouponValues) {
                    var currentValue = 0.0m;
                    couponInfo.IsEffective = TestCouponCriteriaOnLine(lineContext, couponInfo.Coupon)
                        && TryCouponLineValue(
                            couponInfo.Coupon, 
                            postProcessingContext.ShoppingCart,
                            lineContext.CartLine,
                            previousLineValues,
                            out currentValue);
                    // the value computed here is after vat
                    couponInfo.Value = currentValue;
                    previousLineValues.Add(currentValue);
                }
            }
            // compute, for the cart, the contribution of every coupon, in order
            postProcessingContext.BaseCartSubtotal = postProcessingContext.ShoppingCart.Subtotal();
            var previousCartValues = new List<decimal>();
            foreach (var couponInfo in postProcessingContext.CouponValues) {
                var currentValue = 0.0m;
                couponInfo.IsEffective = true
                    && TryCouponCartValue(
                        couponInfo.Coupon,
                        postProcessingContext.ShoppingCart,
                        previousCartValues,
                        out currentValue);
                // the value computed here is after vat
                couponInfo.Value = currentValue;
                previousCartValues.Add(currentValue);
            }
            // TODO
        }

        private void TestPostProcessingCriteria(
            CouponApplicabilityContext context) {

        }
        #endregion

        #region Compute values in cart
        //TODO: we need to have a better way to do the computations
        // for coupons.
        public bool TryCouponCartValue(
            CartPriceAlterationContext context,
            out decimal value) {

            value = 0.0m;
            var coupon = GetCouponFromCode(context.Alteration.Key);
            if (coupon != null) {
                switch (coupon.CouponType) {
                    case CouponType.Percent:
                        // The total amount for the cart is the result of adding up the amounts 
                        // for each line.
                        value = context.ContextsForLines()
                            .Sum(lineContext => {
                                if (!TestCouponCriteriaOnLine(new CouponLineApplicabilityContext {
                                    Coupon = coupon,
                                    CouponCode = coupon.Code,
                                    ShoppingCart = lineContext.ShoppingCart,
                                    CartLine = lineContext.CartLine,
                                    WorkContext = lineContext.WorkContext,
                                    IsApplicable = true
                                }, coupon)) {
                                    return 0.0m;
                                }
                                var partialValue = 0.0m;
                                TryCouponLineValue(lineContext, out partialValue);
                                // apply VAT and such
                                return _productPriceService
                                    .GetPrice(
                                        lineContext.CartLine.Product,
                                        partialValue,
                                        lineContext.ShoppingCart.Country,
                                        lineContext.ShoppingCart.ZipCode);
                            });
                        return true;
                    case CouponType.Amount:
                        // The total amount for the cart is the result of adding up the amounts 
                        // for each line.
                        value = context.ContextsForLines()
                            .Sum(lineContext => {
                                if (!TestCouponCriteriaOnLine(new CouponLineApplicabilityContext {
                                    Coupon = coupon,
                                    CouponCode = coupon.Code,
                                    ShoppingCart = lineContext.ShoppingCart,
                                    CartLine = lineContext.CartLine,
                                    WorkContext = lineContext.WorkContext,
                                    IsApplicable = true
                                }, coupon)) {
                                    return 0.0m;
                                }
                                var partialValue = 0.0m;
                                TryCouponLineValue(lineContext, out partialValue);
                                // apply VAT and such
                                return _productPriceService
                                    .GetPrice(
                                        lineContext.CartLine.Product, 
                                        partialValue, 
                                        lineContext.ShoppingCart.Country, 
                                        lineContext.ShoppingCart.ZipCode);
                            });
                        return true;
                    case CouponType.CartAmount:
                        // for CartAmount type coupons, the total for the cart is already
                        // part of its definition.
                        // this is after VAT
                        value = -coupon.Value;
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        public bool TryCouponLineValue(
            LinePriceAlterationContext context,
            out decimal value) {

            value = 0.0m;
            var coupon = GetCouponFromCode(context.Alteration.Key);
            // we are not testing for the coupon's validity here, we are assuming it
            // should be considered.
            if (coupon != null) {
                var quantity = context.CartLine.Quantity; // TODO: max quantity to consider for coupon
                switch (coupon.CouponType) {
                    case CouponType.Percent:
                        // Consider price as input, before VAT and such
                        var itemPrice = 
                            (context.CartLine.Product.DiscountPrice >= 0 
                                && context.CartLine.Product.DiscountPrice < context.CartLine.Product.Price)
                            ? context.CartLine.Product.DiscountPrice
                            : context.CartLine.Product.Price;
                        var linePrice = GetLinePrice(context.CartLine, quantity, false)
                            + (context.AlterationValues?.Sum(av => av.Value) ?? 0.0m);
                        value = -linePrice * (coupon.Value / 100m);
                        return true;
                    case CouponType.Amount:
                        // Fixed amount discount for each single item. Compute it here before VAT.
                        // coupon.Value is after VAT. Meaning that if you input 1.1, and the VAT is 10%,
                        // this should return (-quantity * 1)
                        var rate = _vatConfigurationService
                            .GetRate(context.CartLine.Product, context.ShoppingCart.Country, context.ShoppingCart.ZipCode);
                        var singleValue = coupon.Value / (1m + rate);
                        value = -quantity * value;
                        return true;
                    case CouponType.CartAmount:
                        // flat coupon on the cart? We need to "spread" its VAT contribution.
                        // Note that this method, for such coupon, is not called when computing
                        // the amount by which the coupon affects the whole cart.
                        value = CartAmountOnLine(
                            coupon,
                            context.ShoppingCart,
                            context.CartLine,
                            context.AlterationValues.Select(av => av.Value));
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        public bool TryCouponCartValue(
            // we are computing stuff for this coupon
            CouponRecord coupon,
            // this is the cart
            IShoppingCart cart,
            // values computed for coupons with higher priority
            IEnumerable<decimal> previousValues,
            // the value contribution of this coupon on the line
            out decimal value) {

            value = 0.0m;
            var workContext = _workContextAccessor.GetContext();

            // Do the computation
            switch (coupon.CouponType) {
                case CouponType.Percent:
                    value = cart.GetProducts()
                        .Sum(cartLine => {
                            if (!TestCouponCriteriaOnLine(new CouponLineApplicabilityContext {
                                Coupon = coupon,
                                CouponCode = coupon.Code,
                                ShoppingCart = cart,
                                CartLine = cartLine,
                                WorkContext = workContext,
                                IsApplicable = true
                            }, coupon)) {
                                return 0.0m;
                            }
                            var partialValue = 0.0m;
                            TryCouponLineValue(coupon, cart, cartLine, null, out partialValue);
                            // apply VAT and such
                            return _productPriceService
                                .GetPrice(cartLine.Product, partialValue, cart.Country, cart.ZipCode);
                        });
                    return true;
                case CouponType.Amount:
                    // The total amount for the cart is the result of adding up the amounts 
                    // for each line.
                    value = cart.GetProducts()
                        .Sum(cartLine => {
                            if (!TestCouponCriteriaOnLine(new CouponLineApplicabilityContext {
                                    Coupon = coupon,
                                    CouponCode = coupon.Code,
                                    ShoppingCart = cart,
                                    WorkContext = workContext
                                }, coupon)) {
                                return 0.0m;
                            }
                            var partialValue = 0.0m;
                            TryCouponLineValue(coupon, cart, cartLine, null, out partialValue);
                            // apply VAT and such
                            return _productPriceService
                                .GetPrice(cartLine.Product, partialValue, cart.Country, cart.ZipCode);
                        });
                    return true;
                case CouponType.CartAmount:
                    // for CartAmount type coupons, the total for the cart is already
                    // part of its definition.
                    // this is after VAT
                    value = -coupon.Value;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryCouponLineValue(
            // we are computing stuff for this coupon
            CouponRecord coupon,
            // this is the cart
            IShoppingCart cart,
            // this is the specific line we are doing our computations on
            ShoppingCartQuantityProduct cartLine,
            // values computed for coupons with higher priority
            IEnumerable<decimal> previousValues,
            // the value contribution of this coupon on the line
            out decimal value) {

            value = 0.0m;

            // we are not testing for the coupon's validity here, we are assuming it
            // should be considered.
            var quantity = cartLine.Quantity; // TODO: max quantity to consider for coupon
            switch (coupon.CouponType) {
                case CouponType.Percent:
                    // Consider price as input, before VAT and such
                    var itemPrice = cartLine.Product.DiscountPrice >= 0 && cartLine.Product.DiscountPrice < cartLine.Product.Price
                        ? cartLine.Product.DiscountPrice
                        : cartLine.Product.Price;
                    var linePrice = GetLinePrice(cartLine, quantity, false)
                        + (previousValues?.Sum() ?? 0.0m);
                    value = -linePrice * (coupon.Value / 100m);
                    return true;
                case CouponType.Amount:
                    // Fixed amount discount for each single item. Compute it here before VAT.
                    // coupon.Value is after VAT. Meaning that if you input 1.1, and the VAT is 10%,
                    // this should return (-quantity * 1)
                    var rate = _vatConfigurationService
                        .GetRate(cartLine.Product, cart.Country, cart.ZipCode);
                    var singleValue = coupon.Value / (1m + rate);
                    value = -quantity * value;
                    return true;
                case CouponType.CartAmount:
                    // flat coupon on the cart? We need to "spread" its VAT contribution.
                    // Note that this method, for such coupon, is not called when computing
                    // the amount by which the coupon affects the whole cart.
                    value = CartAmountOnLine(coupon, cart, cartLine, previousValues);
                    return true;
                default:
                    return false;
            }
        }

        private decimal CartAmountOnLine(
            CouponRecord coupon, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine,
            IEnumerable<decimal> previousValues) {
            // A coupon of this type has its VAT "effect" spread over each line
            // proportionally to the line total over the cart's subtotal

            var quantity = cartLine.Quantity;
            // vat rate for the line
            var rate = _vatConfigurationService
                .GetRate(cartLine.Product, shoppingCart.Country, shoppingCart.ZipCode);
            // products subtotal for the line (after VAT)
            var lineSubtotal = Math.Round(
                _productPriceService.GetPrice(
                        cartLine.Product, cartLine.Price,
                        shoppingCart.Country, shoppingCart.ZipCode)
                * cartLine.Quantity + cartLine.LinePriceAdjustment
                + (previousValues?.Sum() ?? 0.0m), 2);
            // cart products subtotal
            var cartSubtotal = shoppingCart.Subtotal();
            // coupon value spread on this line (after VAT):
            var couponLineValue = (coupon.Value * lineSubtotal) / cartSubtotal;

            return -couponLineValue / (1m + rate); ;
        }

        private decimal GetLinePrice(
            ShoppingCartQuantityProduct cartLine,
            int quantity = -1,
            bool includingVAT = true) {

            var itemPrice = cartLine.Product.DiscountPrice >= 0 && cartLine.Product.DiscountPrice < cartLine.Product.Price
                ? (includingVAT ? _productPriceService.GetDiscountPrice(cartLine.Product) : cartLine.Product.DiscountPrice)
                : (includingVAT ? _productPriceService.GetPrice(cartLine.Product) : cartLine.Product.Price);

            quantity = quantity > 0 ? quantity : cartLine.Quantity;
            return Math.Round(itemPrice * quantity, 2)
                + cartLine.LinePriceAdjustment;
        }

        #endregion

        #region Helpers
        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;
        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }
        // prevent repeating notifications if more processes test the same stuff
        private HashSet<string> _notificationsSent;
        private void Warning(LocalizedString text) {
            if (!_notificationsSent.Contains(text.Text)) {
                _notifier.Warning(text);
                _notificationsSent.Add(text.Text);
            }
        }
        #endregion
    }
}
