using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using System;
using System.Linq;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CartSubTotalCriteria : NumericCriterionBase {

        protected readonly IProductPriceService _productPriceService;

        public CartSubTotalCriteria(
            IProductPriceService productPriceService) {

            _productPriceService = productPriceService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void Describe(DescribeCriterionContext describe) {

            describe.For("Cart", T("Cart Subtotal"), T("Cart Subtotal"))
                .Element("Cart Subtotal",
                    T("Cart Subtotal"),
                    T("Cart Subtotal Criterion"),
                    ApplyCriteria,
                    DisplayCriteria,
                    "NumericFilter");
        }

        public void ApplyCriteria(CriterionContext context) {
            // If the context tells us that already we should not be using
            // the shipping method this criterion is part of, test nothing
            if (context.IsApplicable) {
                // get the comparison func
                var test = Test(context);
                // compute the cart subtotal
                // func for each "line" of price
                var linePrice = LinePrice(
                    context.ApplicabilityContext.Country,
                    context.ApplicabilityContext.ZipCode);
                var subtotal = Math.Round(
                    context.ApplicabilityContext.ProductQuantities
                        .Sum(pq => linePrice(pq)),
                    2);
                // can do the test now
                context.IsApplicable = test(subtotal);
            }
        }

        private Func<ShoppingCartQuantityProduct, decimal> LinePrice(string country, string zipCode) {
            return pq => Math.Round(_productPriceService
                .GetPrice(pq.Product,
                    pq.Price,
                    country,
                    zipCode)
                * pq.Quantity + pq.LinePriceAdjustment, 2);
        }

        public LocalizedString DisplayCriteria(CriterionContext context) {
            return NumericFilterForm.DisplayFilter(T("Cart Subtotal").Text, context.State, T);
        }

    }
}
