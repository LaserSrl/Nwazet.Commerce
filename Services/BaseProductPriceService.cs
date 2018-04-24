using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Commerce")]
    public abstract class BaseProductPriceService : IProductPriceService {
        public virtual decimal GetDiscountPrice(ProductPart part) {
           return part.DiscountPrice;
        }

        public virtual decimal GetPrice(ProductPart part) {
            return part.Price;
        }

        public virtual IEnumerable<PriceTier> GetPriceTiers(ProductPart part) {
            return part.PriceTiers;
        }

        public virtual decimal? GetShippingCost(ProductPart part) {
            return part.ShippingCost;
        }
    }
}
