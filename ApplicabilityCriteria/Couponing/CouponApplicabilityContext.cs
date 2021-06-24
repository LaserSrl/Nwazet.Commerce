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
        public decimal BaseLinePrice { get; set; }

        public override IEnumerable<CouponLineApplicabilityContext> ContextsForLines() {
            yield return this;
        }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponPostApplicabilityContext
        : CouponApplicabilityContext {

        public CouponPostApplicabilityContext(
            IEnumerable<CouponRecord> allCoupons) 
            : base() {

            LineContexts = new List<CouponPostLineApplicabilityContext>();
            AllCoupons = allCoupons.ToList();
            CouponValues = allCoupons
                .Select(cr => new CouponValueInfo { Coupon = cr })
                .ToList();
        }

        public CouponPostApplicabilityContext(
            IEnumerable<CouponRecord> allCoupons,
            CouponApplicabilityContext baseCtx) 
            : this(allCoupons) {

            Coupon = baseCtx.Coupon;
            CouponCode = baseCtx.CouponCode;
            ShoppingCart = baseCtx.ShoppingCart;
            WorkContext = baseCtx.WorkContext;
            IsApplicable = baseCtx.IsApplicable;
            Message = baseCtx.Message;
            ShouldNotify = baseCtx.ShouldNotify;

            LineContexts.AddRange(baseCtx
                .ContextsForLines()
                .Select(lctx => new CouponPostLineApplicabilityContext(allCoupons, lctx)));
        }

        public List<CouponRecord> AllCoupons { get; set; }
        public List<CouponValueInfo> CouponValues { get; set; }
        public List<CouponPostLineApplicabilityContext> LineContexts { get; set; }

        public new IEnumerable<CouponPostLineApplicabilityContext> ContextsForLines() {
            return LineContexts;
        }
    }

    [OrchardFeature("Nwazet.Couponing")]
    public class CouponPostLineApplicabilityContext
        : CouponLineApplicabilityContext {

        public CouponPostLineApplicabilityContext(
            IEnumerable<CouponRecord> allCoupons) : base() {

            AllCoupons = allCoupons.ToList();

            CouponValues = allCoupons
                .Select(cr => new CouponValueInfo { Coupon = cr })
                .ToList();
        }

        public CouponPostLineApplicabilityContext(
            IEnumerable<CouponRecord> allCoupons, 
            CouponLineApplicabilityContext baseCtx) 
            : this(allCoupons) {

            Coupon = baseCtx.Coupon;
            CouponCode = baseCtx.CouponCode;
            ShoppingCart = baseCtx.ShoppingCart;
            WorkContext = baseCtx.WorkContext;
            IsApplicable = baseCtx.IsApplicable;
            Message = baseCtx.Message;
            ShouldNotify = baseCtx.ShouldNotify;
            // stuff specific to lines
            CartLine = baseCtx.CartLine;
            BaseLinePrice = baseCtx.BaseLinePrice;
        }

        public List<CouponRecord> AllCoupons { get; set; }
        public List<CouponValueInfo> CouponValues { get; set; }

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
