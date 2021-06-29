using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {

    public abstract class CartPriceAlterationContextBase {
        public CartPriceAlteration Alteration { get; set; }
        public IShoppingCart ShoppingCart { get; set; }
        public WorkContext WorkContext { get; set; }
        public List<AlterationValueInfo> AlterationValues { get; set; }

        public CartPriceAlterationContextBase() {
            AlterationValues = new List<AlterationValueInfo>();
        }

        public abstract IEnumerable<LinePriceAlterationContext> ContextsForLines();

        public virtual void SetAlteration(CartPriceAlteration alteration) {
            Alteration = alteration;
            foreach (var lc in ContextsForLines()) {
                lc.SetAlteration(alteration);
            }
        }
    }
    public class CartPriceAlterationContext
        : CartPriceAlterationContextBase {

        public CartPriceAlterationContext() : base() {
            _lineContexts = new Dictionary<string, LinePriceAlterationContext>();
        }

        protected Dictionary<string, LinePriceAlterationContext> _lineContexts;

        public override IEnumerable<LinePriceAlterationContext> ContextsForLines() {
            if (ShoppingCart != null) {
                var lines = ShoppingCart.GetProducts();
                if (lines != null) {
                    foreach (var line in lines) {
                        var key = line.GenerateUniqueKey();
                        if (!_lineContexts.ContainsKey(key)) {
                            _lineContexts.Add(
                                key,
                                new LinePriceAlterationContext {
                                    // everything is the same as this context
                                    Alteration = this.Alteration,
                                    ShoppingCart = this.ShoppingCart,
                                    WorkContext = this.WorkContext,
                                    // and finally we consider the line
                                    CartLine = line
                                });
                        }
                        yield return _lineContexts[key];
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
        public override void SetAlteration(CartPriceAlteration alteration) {
            Alteration = alteration;
        }
    }

    public class AlterationValueInfo {
        public CartPriceAlteration Alteration { get; set; }
        public decimal Value { get; set; }
        public bool Effective { get; set; }
    }
}
