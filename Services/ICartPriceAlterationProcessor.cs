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
        /// <param name="alteration"></param>
        /// <returns>Returns the amount by which the price should change 
        /// (i.e. not a percentage). This amount is already affected by VAT
        /// (meaning it includes VAT).</returns>
        decimal AlterationAmount(
            CartPriceAlteration alteration, 
            IShoppingCart shoppingCart,
            IEnumerable<CartPriceAlterationAmount> previousAmounts = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationLabel(
            CartPriceAlteration alteration, 
            IShoppingCart shoppingCart);

        /// <summary>
        /// Compute the price change related to a single cart line.
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns>Returns the amount by which the price should change 
        /// (i.e. not a percentage). This amount is before VAT is applied.</returns>
        decimal AlterationAmount(
            CartPriceAlteration alteration, 
            IShoppingCart shoppingCart, 
            ShoppingCartQuantityProduct cartLine,
            IEnumerable<CartPriceAlterationAmount> previousAmounts = null, 
            bool force = false);

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

    public abstract class CartPriceAlterationContextBase {
        public CartPriceAlteration Alteration { get; set; }
        public IShoppingCart ShoppingCart { get; set; }
        public WorkContext WorkContext { get; set; }
        public List<AlterationValueInfo> AlterationValues { get; set; }

        public CartPriceAlterationContextBase() {
            AlterationValues = new List<AlterationValueInfo>();
        }

        public abstract IEnumerable<LinePriceAlterationContext> ContextsForLines();
    }
    public class CartPriceAlterationContext 
        : CartPriceAlterationContextBase {

        public CartPriceAlterationContext() : base() { }

        public override IEnumerable<LinePriceAlterationContext> ContextsForLines() {
            if (ShoppingCart != null) {
                var lines = ShoppingCart.GetProducts();
                if (lines != null) {
                    foreach (var line in lines) {
                        yield return new LinePriceAlterationContext {
                            // everything is the same as this context
                            Alteration = this.Alteration,
                            ShoppingCart = this.ShoppingCart,
                            WorkContext = this.WorkContext,
                            // and finally we consider the line
                            CartLine = line
                        };
                    }
                }
            }
        }
    }
    public class LinePriceAlterationContext 
        : CartPriceAlterationContextBase {
        public ShoppingCartQuantityProduct CartLine { get; set; }

        public LinePriceAlterationContext() : base() { }

        public override IEnumerable<LinePriceAlterationContext> ContextsForLines() {
            yield return this;
        }
    }

    public class AlterationValueInfo {
        public CartPriceAlteration Alteration { get; set; }
        public decimal Value { get; set; }
    }
}
