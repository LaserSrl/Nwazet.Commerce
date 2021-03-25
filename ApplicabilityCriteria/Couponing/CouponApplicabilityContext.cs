using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicabilityContext {

        public CouponApplicabilityContext() {
            ShouldNotify = true;
        }

        public CouponRecord Coupon { get; set; }
        public string CouponCode { get; set; }
        public IShoppingCart ShoppingCart { get; set; }
        public WorkContext WorkContext { get; set; }
        public bool IsApplicable { get; set; }
        public LocalizedString Message { get; set; }

        // flag telling whether we should attempt to put out a notification
        // to warn users in case a step fails
        public bool ShouldNotify { get; set; }

        public virtual IEnumerable<CouponLineApplicabilityContext> ContextsForLines() {
            if (ShoppingCart != null) {
                var lines = ShoppingCart.GetProducts();
                if (lines != null) {
                    foreach (var line in lines) {
                        yield return new CouponLineApplicabilityContext {
                            // everything is the same as this context
                            Coupon = this.Coupon,
                            CouponCode = this.CouponCode,
                            ShoppingCart = this.ShoppingCart,
                            WorkContext = this.WorkContext,
                            IsApplicable = this.IsApplicable,
                            Message = this.Message,
                            ShouldNotify = this.ShouldNotify,
                            // and finally we consider the line
                            CartLine = line
                        };
                    }
                }
            }
        }
    }


    [OrchardFeature("Nwazet.Couponing")]
    public class CouponLineApplicabilityContext
        : CouponApplicabilityContext {
        public CouponLineApplicabilityContext() : base() {
            // Initialize ShouldNotify at false to prevent notifications that
            // would warn a user a coupon doesn't apply, when in reality it
            // applies to only part of the cart.
            ShouldNotify = false;
        }
        public ShoppingCartQuantityProduct CartLine { get; set; }

        public override IEnumerable<CouponLineApplicabilityContext> ContextsForLines() {
            yield return this;
        }
    }
}
