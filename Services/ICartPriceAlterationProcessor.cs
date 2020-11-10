using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface ICartPriceAlterationProcessor : IDependency {
        string AlterationType { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns>Returns the amount by which the price should change 
        /// (i.e. not a percentage).</returns>
        decimal EvaluateAlteration(CartPriceAlteration alteration, IShoppingCart shoppingCart);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationLabel(CartPriceAlteration alteration, IShoppingCart shoppingCart);

        bool CanProcess(CartPriceAlteration alteration, IShoppingCart shoppingCart);
    }
}
