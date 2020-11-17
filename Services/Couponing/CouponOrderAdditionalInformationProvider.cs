using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
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
            var cart = context.ShoppingCart;
            var couponAlterations = cart
                ?.PriceAlterations
                ?.Where(pa => CouponingUtilities.CouponAlterationType
                    .Equals(pa.AlterationType));
            if (couponAlterations != null) {
                foreach (var alteration in couponAlterations) {
                    // each element we create here will need to contain sufficient information
                    // for us to completely recompute everything about this coupon later when
                    // the order is fetched anew.
                    var coupon = GetCouponFromCode(alteration.Key);
                    if (coupon != null) { // sanity check
                        var xCoupon = coupon.ToXMLElement();
                        // the coupon itself will also be in the AdditionalElements property of the order
                        yield return xCoupon; 
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
                            // avoid potentially recomputing lines for each processor
                            var productLines = cart.GetProducts();
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
                                var values = processors
                                    .Select(p => new OrderInformationDetail {
                                        Label = p.AlterationLabel(alteration, cart, productLine),
                                        Value = p.AlterationAmount(alteration, cart, productLine),
                                        ValueType = OrderValueType.Currency,
                                        InformationType = OrderInformationType.RawLinePrice,
                                        ProcessorClass = p.GetType().FullName
                                    })
                                    .Where(o => !string.IsNullOrWhiteSpace(o.Label));
                                if (values.Any()) {
                                    var orderLinealteration = new OrderLineInformation() {
                                        ProductId = productLine.Product.Id,
                                        Details = values,
                                        Source = xCoupon
                                    };
                                    yield return orderLinealteration.ToXML();
                                }
                            }
                            //  - By what "value" does the coupon affect the cart as a whole?
                            // A coupon set as a % amount is applied, for the order, on each line, rather than
                            // on the whole cart, so it will not introduce an additional element here. On the other
                            // hand, a coupon for a flat amount (e.g. "-10€") would be here.
                            // TODO: do this, paying attention to taxable amounts and VAT

                            // "summary" element
                            yield return new OrderAdditionalInformation() {
                                Source = xCoupon,
                                Details = processors.Select(p =>
                                    new OrderInformationDetail {
                                        Label = p.AlterationLabel(alteration, cart),
                                        Value = p.AlterationAmount(alteration, cart),
                                        Description = coupon.ToString(),
                                        ValueType = OrderValueType.Currency,
                                        InformationType = OrderInformationType.TextInfo,
                                        ProcessorClass = p.GetType().FullName
                                    })
                            }.ToXML();
                            yield return new OrderAdditionalInformation() {
                                Source = xCoupon,
                                Details = processors.Select(p =>
                                    new OrderInformationDetail {
                                        Label = p.AlterationLabel(alteration, cart),
                                        Value = p.AlterationAmount(alteration, cart),
                                        Description = coupon.ToString(),
                                        ValueType = OrderValueType.Currency,
                                        InformationType = OrderInformationType.FrontEndInfo,
                                        ProcessorClass = p.GetType().FullName
                                    })
                            }.ToXML();
                        }
                        // we need to also return the XElement that will be used in frontend to
                        // report to the customer that they have used the coupon

                        //// we need to add to this XElement all the "dynamic" results of having 
                        //// the coupon in the current context.
                        //if (processors.Any()) {
                            
                        //    //  - By what "value" does the coupon affect the cart as a whole?
                        //    var cartValues = processors
                        //        .Select(p => new {
                        //            Label = p.AlterationLabel(alteration, cart),
                        //            Value = p.AlterationAmount(alteration, cart),
                        //            AlterationType = p.AlterationType,
                        //            ProcessorClass = p.GetType().FullName
                        //        })
                        //        .Where(o => o.Value != 0.0m);
                        //    if (cartValues.Any()) {
                        //        // there are values affected by the coupon. We add XML like this:
                        //        // <CartAlteration">
                        //        //   <AlterationDetali 
                        //        //     Label ="{Label computed}" 
                        //        //     Value ="{alteration amount}"
                        //        //     AlterationType = "Coupon" 
                        //        //     ProcessorClass = "{name of the C# class for the processor}" />
                        //        // </CartAlteration>
                        //        // For each line in the order there may be one LineAlteration element, containing
                        //        // one AlterationDetail element for each processor that gave results for
                        //        // the coupon.
                        //        xCoupon
                        //            .AddEl(new XElement("CartAlteration",
                        //                cartValues.Select(v =>
                        //                    new XElement("AlterationDetail")
                        //                        .Attr("Label", v.Label)
                        //                        .Attr("Value", v.Value)
                        //                        .Attr("AlterationType", v.AlterationType)
                        //                        .Attr("ProcessorClass", v.ProcessorClass))));
                        //    }
                        //}
                        //yield return xCoupon;
                    }
                }
            }
        }

    }
}
