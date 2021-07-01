using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
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
        /// This may be the sum of the alterations to single products, or something
        /// unrelated to it.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns the amount by which the price should change 
        /// (i.e. not a percentage). This amount is already affected by VAT
        /// (meaning it includes VAT). The context object will also contain 
        /// the price changes for each line. Those amount s are before VAT
        /// is applied.</returns>
        decimal AlterationAmount(CartPriceAlterationContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationLabel(
            CartPriceAlteration alteration, 
            IShoppingCart shoppingCart);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationLabel(
            CartPriceAlteration alteration, 
            IShoppingCart shoppingCart, 
            ShoppingCartQuantityProduct cartLine);

        /// <summary>
        /// Tells whether an implementation is able to handle the given alteration.
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        bool CanProcess(CartPriceAlteration alteration);

        bool CanProcess(CartPriceAlteration alteration, IShoppingCart shoppingCart);
    }

}
