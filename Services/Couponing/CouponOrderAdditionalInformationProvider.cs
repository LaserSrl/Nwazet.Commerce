using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponOrderAdditionalInformationProvider :
        BaseOrderAdditionalInformationProvider {

        private readonly ICouponRepositoryService _couponRepositoryService;
        protected readonly IEnumerable<ICartPriceAlterationProcessor> _cartPriceAlterationProcessors;

        public CouponOrderAdditionalInformationProvider(
            ICouponRepositoryService couponRepositoryService,
            IEnumerable<ICartPriceAlterationProcessor> cartPriceAlterationProcessors) {

            _couponRepositoryService = couponRepositoryService;
            _cartPriceAlterationProcessors = cartPriceAlterationProcessors;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
        }

        public override void StoreAdditionalInformation(OrderPart orderPart) {
            // we must store information about the coupons being used in the order
            // but for us to store it here, we need that information to already
            // be in the Order somewhere. Unlike VAT, whose information can be 
            // found in the products, coupons are an object in the shopping cart,
            // and should be added separately to the order. 
            // OrderPartRecord has the Contents string, where we store an xml
            // document describing the order itself. That is a good place to store
            // the coupons' information, but that has to be done before getting to
            // this call, when the OrderPart is being built. So look at the
            // PrepareAdditionalInformation method.
        }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;
        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }

        public override IEnumerable<XElement> PrepareAdditionalInformation(OrderContext context) {
            // TODO: there may be alterations that are not coupons. This provider should not ignore
            // them. Right now, it's not considering them when evaluating previous values to be used
            // by coupons' processing. It should, but without saving other information into the order
            // (that should probably be handled by its own provider)
            var cart = context.ShoppingCart;
            var couponAlterations = cart
                // PriceAlterations is already ordered by descending Weight.
                ?.PriceAlterations
                ?.Where(pa => CouponingUtilities.CouponAlterationType
                    .Equals(pa.AlterationType));
            if (couponAlterations != null) {
                // avoid potentially recomputing lines for each processor
                var productLines = cart.GetProducts();
                // Each alteration may affect the computation for the next.
                // For each line, we are going to store the "effects" of already computed
                // coupons.
                var previousLineAlterations = new Dictionary<string, List<CartPriceAlterationAmount>>();
                foreach (var pLine in productLines) {
                    // initialize dictionary so we don't have to check that the list exists when we
                    // actually use it.
                    previousLineAlterations.Add(pLine.GenerateUniqueKey(), new List<CartPriceAlterationAmount>());
                }
                var previousSummaryAlterations = new List<CartPriceAlterationAmount>();
                foreach (var alteration in couponAlterations) {
                    // each element we create here will need to contain sufficient information
                    // for us to completely recompute everything about this coupon later when
                    // the order is fetched anew.
                    var coupon = GetCouponFromCode(alteration.Key);
                    if (coupon != null) { // sanity check
                        // The coupon should potentially add several XElements:
                        // - 1 element containing "summary" information, telling a coupon was there
                        // - 0+ LineAlteration elements, that apply to a single CheckoutItem
                        // - 0+ OrderAlteration elements, that apply to the order as a whole
                        // - 0+ other? TODO
                        // These elements should contain all the information that will be
                        // required to eventually repeat their computations, but also the 
                        // results.

                        // get the processors that are able to manipulate and evaluate the coupon:
                        var processors = _cartPriceAlterationProcessors
                            .Where(cpap => cpap.CanProcess(alteration, cart));
                        if (processors.Any()) {
                            var xCoupon = coupon.ToXMLElement();
                            // the coupon itself will also be in the AdditionalElements property of the order
                            yield return xCoupon;

                            //  - What products of the cart, if any, does the coupon affect?
                            // List the ids if all affected products. This may contain no ids in case
                            // the coupon is of specific "types", e.g. when it's a coupon for free shipping.
                            //  - For each of the products the coupon affects, what is the "value"
                            // it affects it by?
                            // This should be the "line value". Basically, how the coupon affects the whole
                            // line of the order. A simple example:
                            // Product with id 42; it's price is 50€. The coupon is a 10% discount on it.
                            // If the quantity for product42 is 1, the coupon value for the line is 5€;
                            // If the quantity for product42 is higher, the coupon value for the line is 5€ * quantity.
                            // This may not always be the case. For example, a coupon may give a single free
                            // product42 if at least 5 are being payed. In that case, whenever quantity is 
                            // >5 the value of the coupon for the line will be 50€. Perhaps we should also
                            // add the fact that we are adding to the quantity?

                            foreach (var productLine in productLines) {
                                var lineKey = productLine.GenerateUniqueKey();
                                var lineValues = new List<OrderInformationDetail>();
                                foreach (var processor in processors) {
                                    var lineLabel = processor.AlterationLabel(alteration, cart, productLine);
                                    var lineAmount = processor.AlterationAmount(alteration, cart, productLine,
                                        previousLineAlterations[lineKey], 
                                        // for coupons of type CartAmount, we need to enforce doing the computations
                                        // also for lines the coupon would normally not apply to.
                                        coupon.CouponType == CouponType.CartAmount);
                                    // add "new" result to list
                                    previousLineAlterations[lineKey].Add(new CartPriceAlterationAmount() {
                                        Amount = lineAmount,
                                        // we probably don't even need this next few properties for any
                                        // computation here
                                        Label = lineLabel,
                                        AlterationType = alteration.AlterationType,
                                        Key = alteration.Key,
                                        Weight = alteration.Weight,
                                        RemovalAction = alteration.RemovalAction
                                    });
                                    // add "new" results to list that will be in order XML
                                    lineValues.Add(new OrderInformationDetail() {
                                        Label = lineLabel,
                                        Value = lineAmount,
                                        ValueType = OrderValueType.Currency,
                                        InformationType = OrderInformationType.RawLinePrice,
                                        ProcessorClass = processor.GetType().FullName
                                    });
                                }
                                if (lineValues.Any()) {
                                    var orderLinealteration = new OrderLineInformation() {
                                        ProductId = productLine.Product.Id,
                                        LineKey = productLine.GenerateUniqueKey(),
                                        Details = lineValues,
                                        Source = xCoupon
                                    };
                                    yield return orderLinealteration.ToXML();
                                }
                            }

                            var feDetails = new List<OrderInformationDetail>();
                            var beDetails = new List<OrderInformationDetail>();
                            foreach (var processor in processors) {
                                var cartLabel = processor.AlterationLabel(alteration, cart);
                                var cartAmount = processor.AlterationAmount(alteration, cart, previousSummaryAlterations);
                                var processorClass = processor.GetType().FullName;
                                // add "new" results to lists
                                previousSummaryAlterations.Add(new CartPriceAlterationAmount() {
                                    Amount = cartAmount,
                                    // we probably don't even need this next few properties for any
                                    // computation here
                                    Label = cartLabel,
                                    AlterationType = alteration.AlterationType,
                                    Key = alteration.Key,
                                    Weight = alteration.Weight,
                                    RemovalAction = alteration.RemovalAction
                                });
                                feDetails.Add(new OrderInformationDetail {
                                    Label = cartLabel,
                                    Value = cartAmount,
                                    Description = coupon.ToString(),
                                    ValueType = OrderValueType.Currency,
                                    InformationType = OrderInformationType.TextInfo,
                                    ProcessorClass = processorClass
                                });
                                beDetails.Add(new OrderInformationDetail {
                                    Label = cartLabel,
                                    Value = cartAmount,
                                    Description = coupon.ToString(),
                                    ValueType = OrderValueType.Currency,
                                    InformationType = OrderInformationType.FrontEndInfo,
                                    ProcessorClass = processorClass
                                });
                            }
                            // "summary" element for backend
                            if (beDetails != null && beDetails.Any()) {
                                yield return new OrderAdditionalInformation() {
                                    Source = xCoupon,
                                    Details = beDetails
                                }.ToXML();
                            }
                            // "summary" element for frontend
                            if (feDetails != null && feDetails.Any()) {
                                yield return new OrderAdditionalInformation() {
                                    Source = xCoupon,
                                    Details = feDetails
                                }.ToXML();
                            }
                        } else {
                            // This coupon had no effect on the cart/order. Perhaps the user
                            // added it and then changed something in the context such that the
                            // coupon was not valid anymore. For example, the coupon was only 
                            // for anonymous users, then the user logged in.
                            // We still want to store this information.
                            var xCoupon = coupon.ToXMLElement(true);
                            // the coupon itself will also be in the AdditionalElements property of the order
                            yield return xCoupon;
                        }
                    }
                }
            }
        }

    }
}
