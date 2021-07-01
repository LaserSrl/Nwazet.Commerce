using Nwazet.Commerce.Extensions;
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

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponPostApplicabilityContext
        : CouponApplicabilityContext {

        public List<CouponValueInfo> CouponValues { get; set; }
        public decimal BaseCartSubtotal { get; set; }

        protected Dictionary<string, CouponPostLineApplicabilityContext> _lineContexts;

        protected CouponPostApplicabilityContext()
            : base() {

            CouponValues = new List<CouponValueInfo>();
            _lineContexts = new Dictionary<string, CouponPostLineApplicabilityContext>();
        }

        public CouponPostApplicabilityContext(
            CouponApplicabilityContext baseCtx)
            : this() {

            Coupon = baseCtx.Coupon;
            CouponCode = baseCtx.CouponCode;
            ShoppingCart = baseCtx.ShoppingCart;
            WorkContext = baseCtx.WorkContext;
            IsApplicable = baseCtx.IsApplicable;
            Message = baseCtx.Message;
            ShouldNotify = baseCtx.ShouldNotify;

            foreach (var lctx in baseCtx.ContextsForLines()) {
                _lineContexts.Add(
                    lctx.CartLine.GenerateUniqueKey(),
                    new CouponPostLineApplicabilityContext(lctx));
            }
        }

        public virtual void SetCoupon(CouponRecord coupon) {
            Coupon = coupon;
            CouponCode = coupon.Code;
            foreach (var lc in ContextsForLines()) {
                lc.SetCoupon(coupon);
            }
        }

        public new IEnumerable<CouponPostLineApplicabilityContext> ContextsForLines() {
            if (ShoppingCart != null) {
                var lines = ShoppingCart.GetProducts();
                if (lines != null) {
                    foreach (var line in lines) {
                        var key = line.GenerateUniqueKey();
                        if (!_lineContexts.ContainsKey(key)) {
                            _lineContexts.Add(
                                key,
                                new CouponPostLineApplicabilityContext {
                                    // everything is the same as this context
                                    Coupon = this.Coupon,
                                    CouponCode = this.CouponCode,
                                    ShoppingCart = this.ShoppingCart,
                                    WorkContext = this.WorkContext,
                                    IsApplicable = true, // default to true otherwise no test will be performed
                                    ShouldNotify = this.ShouldNotify,
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

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponPostLineApplicabilityContext
        : CouponLineApplicabilityContext {

        public List<CouponValueInfo> CouponValues { get; set; }
        public decimal BaseLinePrice { get; set; }

        public CouponPostLineApplicabilityContext()
            : base() {

            CouponValues = new List<CouponValueInfo>();
        }

        public CouponPostLineApplicabilityContext(
            CouponLineApplicabilityContext baseCtx)
            : this() {

            Coupon = baseCtx.Coupon;
            CouponCode = baseCtx.CouponCode;
            ShoppingCart = baseCtx.ShoppingCart;
            WorkContext = baseCtx.WorkContext;
            IsApplicable = baseCtx.IsApplicable;
            Message = baseCtx.Message;
            ShouldNotify = baseCtx.ShouldNotify;
            // stuff specific to lines
            CartLine = baseCtx.CartLine;
        }


        public List<CouponRecord> AllCoupons { get; set; }

        public virtual void SetCoupon(CouponRecord coupon) {
            Coupon = coupon;
            CouponCode = coupon.Code;
        }

        public new IEnumerable<CouponPostLineApplicabilityContext> ContextsForLines() {
            yield return this;
        }
    }

    public class CouponValueInfo {
        public CouponRecord Coupon { get; set; }
        public bool IsEffective { get; set; }
        public decimal Value { get; set; }
    }
}
