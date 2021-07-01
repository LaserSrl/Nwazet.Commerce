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

                // new processor style
                var alterationContext = new CartPriceAlterationContext {
                    ShoppingCart = context.ShoppingCart,
                    WorkContext = context.WorkContextAccessor.GetContext()
                };
                foreach (var alt in cart.PriceAlterations) {
                    alterationContext.SetAlteration(alt);
                    // get processors that are able to process the alteration
                    var processors = _cartPriceAlterationProcessors
                        .Where(p => p.CanProcess(alt));
                    foreach (var processor in processors) {
                        processor.AlterationAmount(alterationContext);
                    }
                }
                foreach (var couponAlteration in couponAlterations) {
                    var coupon = GetCouponFromCode(couponAlteration.Key);
                    if (coupon != null) { // sanity check
                        var xCoupon = coupon.ToXMLElement();
                        // the coupon itself will also be in the AdditionalElements property of the order
                        yield return xCoupon;
                        // from the context we filled in above, extract all alteration values for this coupon
                        // for the lines:
                        foreach (var productLine in productLines) {
                            var lineKey = productLine.GenerateUniqueKey();
                            var lineCtx = alterationContext.GetContextForLine(lineKey);
                            var lineValuesForAlteration = lineCtx
                                .AlterationValues
                                .Where(av => CouponingUtilities.CouponAlterationType.Equals(av.Alteration.AlterationType)
                                    && av.Alteration.Key == couponAlteration.Key);
                            if (lineValuesForAlteration.Any()) {
                                yield return new OrderLineInformation() {
                                    ProductId = productLine.Product.Id,
                                    LineKey = lineKey,
                                    Details = lineValuesForAlteration
                                        .Select(av => new OrderInformationDetail() {
                                            Label = av.Label,
                                            Value = av.Value,
                                            ValueType = OrderValueType.Currency,
                                            InformationType = OrderInformationType.RawLinePrice,
                                            ProcessorClass = av.ProcessorClass
                                        }),
                                    Source = xCoupon
                                }.ToXML();
                            }
                        }
                        // for the whole cart:
                        var cartValuesForAlteration = alterationContext
                            .AlterationValues
                            .Where(av => CouponingUtilities.CouponAlterationType.Equals(av.Alteration.AlterationType)
                                && av.Alteration.Key == couponAlteration.Key);
                        if (cartValuesForAlteration.Any()) {
                            // backend info
                            yield return new OrderAdditionalInformation() {
                                Source = xCoupon,
                                Details = cartValuesForAlteration
                                    .Select(av => new OrderInformationDetail() {
                                        Label = av.Label,
                                        Value = av.Value,
                                        Description = coupon.ToString(),
                                        ValueType = OrderValueType.Currency,
                                        InformationType = OrderInformationType.TextInfo,
                                        ProcessorClass = av.ProcessorClass
                                    })
                            }.ToXML();
                            // frontend info
                            yield return new OrderAdditionalInformation() {
                                Source = xCoupon,
                                Details = cartValuesForAlteration
                                    .Select(av => new OrderInformationDetail() {
                                        Label = av.Label,
                                        Value = av.Value,
                                        Description = coupon.ToString(),
                                        ValueType = OrderValueType.Currency,
                                        InformationType = OrderInformationType.FrontEndInfo,
                                        ProcessorClass = av.ProcessorClass
                                    })
                            }.ToXML();
                        }
                    }
                }
            }
        }

    }
}
