using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Commerce")]
    public class PricePart : ContentPart<PricePartRecord> {
        /// <summary>
        /// This will be the price evaluated after promotions, discounts
        /// and such.
        /// </summary>
        public decimal EffectiveUnitPrice {
            get { return Retrieve(r => r.EffectiveUnitPrice); }
            set { Store(r => r.EffectiveUnitPrice, value); }
        }

        public decimal FrontendUnitPrice {
            get {
                var product = this.As<ProductPart>();
                if (product != null && product.ProductPriceService != null) {
                    return product.ProductPriceService.GetPrice(product, EffectiveUnitPrice);
                }
                return EffectiveUnitPrice;
            }
        }
    }
}
