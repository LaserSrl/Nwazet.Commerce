using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatTaxComputationHelper : ITaxComputationHelper {

        private readonly IVatConfigurationService _vatConfigurationService;

        public VatTaxComputationHelper(
            IVatConfigurationService vatConfigurationService) {

            _vatConfigurationService = vatConfigurationService;
        }

        public decimal ComputeTax(ITax tax, TaxContext context) {
            if (tax == null || context == null) {
                return 0;
            }

            if (tax.GetType() == typeof(VatConfigurationPart)) {
                var destination = context.DestinationTerritory;
                // if destination is null, we may still pass it to the methods from IVatConfigurationService,
                // that will treat it as the default destination.

                // In these computations we don't care about the ITax object anymore. Each product
                // has its own VAT category, which will be used in computing the taxes. The issue with
                // this approach would be that we would end up computing the same things over and over 
                // for each VAT categoy configured. That is why in VatConfigurationProvider.GetTaxes()
                // we cheat and only return the first VatConfigurationPart.

                // This method should return the total tax for all the products from the context.
                return context
                    .ShoppingCartQuantityProducts
                    .Sum(scqp => {
                        var rate = _vatConfigurationService.GetRate(scqp.Product, destination);
                        return
                            rate *
                                (scqp.Quantity * (scqp.Price)
                                + scqp.LinePriceAdjustment);
                    });
            }

            return 0;
        }
    }
}
