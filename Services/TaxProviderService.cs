using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Taxes")]
    public class TaxProviderService : ITaxProviderService {

        private readonly IEnumerable<ITaxComputationHelper> _taxComputationHelpers;

        public TaxProviderService(
            IEnumerable<ITaxComputationHelper> taxComputationHelpers) {

            _taxComputationHelpers = taxComputationHelpers;
        }

        public TaxContext CreateContext(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities,
            decimal subtotal,
            decimal shippingCost,
            string country, string zipCode) {

            return new TaxContext() {
                ShoppingCartQuantityProducts = productQuantities,
                CartSubTotal = subtotal,
                ShippingPrice = shippingCost,
                Country = country,
                ZipCode = zipCode
            };
        }

        public decimal TotalTaxes(ITax tax, TaxContext context) {
            return _taxComputationHelpers.Sum(tch => tch.ComputeTax(tax, context));
        }

        public IEnumerable<decimal> ItemizedTaxes(ITax tax, TaxContext context) {
            foreach (var productQuantity in context.ShoppingCartQuantityProducts) {
                var singleItemContext = CreateContext(
                    new ShoppingCartQuantityProduct[] { productQuantity },
                    productQuantity.Price * productQuantity.Quantity + productQuantity.LinePriceAdjustment,
                    0,
                    context.Country,
                    context.ZipCode);
                yield return _taxComputationHelpers.Sum(tch => tch.ComputeTax(tax, singleItemContext));
            }
        }

        public decimal ShippingTaxes(ITax tax, TaxContext context) {
            var shippingContext = CreateContext(
                null,
                0,
                context.ShippingPrice,
                context.Country,
                context.ZipCode
                );
            return _taxComputationHelpers.Sum(tch => tch.ComputeTax(tax, shippingContext));
        }
    }
}
