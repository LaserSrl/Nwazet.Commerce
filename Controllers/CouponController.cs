using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponController : Controller {

        private readonly ICouponApplicationService _couponApplicationService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CouponController(
            ICouponApplicationService couponApplicationService,
            IShoppingCart shoppingCart,
            IWorkContextAccessor workContextAccessor) {

            _couponApplicationService = couponApplicationService;
            _shoppingCart = shoppingCart;
            _workContextAccessor = workContextAccessor;
        }

        [HttpPost]
        public ActionResult Apply(CouponFrontendViewModel coupon) {

            if (coupon != null && !string.IsNullOrWhiteSpace(coupon.Code)) {
                var context = new CouponApplicabilityContext {
                    CouponCode = coupon.Code,
                    ShoppingCart = _shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                };
                _couponApplicationService.ApplyCoupon(context);
            }

            return RedirectToAction("Index", "ShoppingCart");
        }

        [HttpPost]
        public ActionResult Remove(CouponFrontendViewModel coupon) {

            if (coupon != null && !string.IsNullOrWhiteSpace(coupon.Code)) {
                var context = new CouponApplicabilityContext {
                    CouponCode = coupon.Code,
                    ShoppingCart = _shoppingCart,
                    WorkContext = _workContextAccessor.GetContext(),
                };
                _couponApplicationService.RemoveCoupon(context);
            }

            return RedirectToAction("Index", "ShoppingCart");
        }
    }
}
