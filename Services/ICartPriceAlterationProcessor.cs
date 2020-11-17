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
        /// Compute the cart's total price change due to a CartPriceAlteration object.
        /// This may be the sum of the alterations to sinlg eproducts, or something
        /// unrelated to it.
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns>Returns the amount by which the price should change 
        /// (i.e. not a percentage).</returns>
        decimal AlterationAmount(CartPriceAlteration alteration, IShoppingCart shoppingCart);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationLabel(CartPriceAlteration alteration, IShoppingCart shoppingCart);

        /// <summary>
        /// Compute the price change related to a single cart line.
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns>Returns the amount by which the price should change 
        /// (i.e. not a percentage).</returns>
        decimal AlterationAmount(CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationLabel(CartPriceAlteration alteration, IShoppingCart shoppingCart, ShoppingCartQuantityProduct cartLine);

        bool CanProcess(CartPriceAlteration alteration, IShoppingCart shoppingCart);
    }
}
