using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IProductPriceService : IDependency {
        /// <summary>
        /// Returns the price for the product
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        decimal GetPrice(ProductPart part);

        /// <summary>
        /// Returns the discounted price for the product
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        decimal GetDiscountPrice(ProductPart part);

        /// <summary>
        /// Returns the shipping cost for the product
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        decimal? GetShippingCost(ProductPart part);

        /// <summary>
        /// Returns the Price tiers for the product
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        IEnumerable<PriceTier> GetPriceTiers(ProductPart part);
    }
}
