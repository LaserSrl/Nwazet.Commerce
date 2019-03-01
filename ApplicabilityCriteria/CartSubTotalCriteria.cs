using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CartSubTotalCriteria : IApplicabilityCriterionProvider {

        protected readonly IProductPriceService _productPriceService;

        public CartSubTotalCriteria(
            IProductPriceService productPriceService) {

            _productPriceService = productPriceService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeCriterionContext describe) {

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
                // We used NumericFilterForm for the form. That class does not provide us
                // with a method to do the comparison ourselves, so we have to do it here
                // explicitly, replicating some of the stuff that's there.
                var op = (NumericOperator)Enum.Parse(
                    typeof(NumericOperator), 
                    Convert.ToString(context.State.Operator));
                decimal min, max;
                if (op == NumericOperator.Between || op == NumericOperator.NotBetween) {
                    min = decimal.Parse(
                        Convert.ToString(context.State.Min), 
                        CultureInfo.InvariantCulture);
                    max = decimal.Parse(
                        Convert.ToString(context.State.Max), 
                        CultureInfo.InvariantCulture);
                } else {
                    min = max = decimal.Parse(
                        Convert.ToString(context.State.Value), 
                        CultureInfo.InvariantCulture);
                }
                // get the comparison func
                var test = Test(op, min, max);
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

        private Func<decimal, bool> Test(NumericOperator op, decimal min, decimal max) {
            switch (op) {
                case NumericOperator.LessThan:
                    return x => x < max;
                case NumericOperator.LessThanEquals:
                    return x => x <= max;
                case NumericOperator.Equals:
                    if (min == max) {
                        return x => x == min;
                    }
                    return x => x >= min && x <= max;
                case NumericOperator.NotEquals:
                    if (min == max) {
                        return x => x != min;
                    }
                    return x => x < min || x > max;
                case NumericOperator.GreaterThan:
                    return x => x > min;
                case NumericOperator.GreaterThanEquals:
                    return x => x >= min;
                case NumericOperator.Between:
                    return x => x >= min && x <= max;
                case NumericOperator.NotBetween:
                    return x => x < min || x > max;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public LocalizedString DisplayCriteria(CriterionContext context) {
            return NumericFilterForm.DisplayFilter(T("Cart Subtotal").Text, context.State, T);
        }

    }
}
