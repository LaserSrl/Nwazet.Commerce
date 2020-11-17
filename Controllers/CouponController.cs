using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
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

        public CouponController(
            ICouponApplicationService couponApplicationService) {

            _couponApplicationService = couponApplicationService;
        }

        [HttpPost]
        public ActionResult Apply(CouponFrontendViewModel coupon) {

            if (coupon != null && !string.IsNullOrWhiteSpace(coupon.Code)) {
                _couponApplicationService.ApplyCoupon(coupon.Code);
            }

            return RedirectToAction("Index", "ShoppingCart");
        }

        [HttpPost]
        public ActionResult Remove(CouponFrontendViewModel coupon) {

            if (coupon != null && !string.IsNullOrWhiteSpace(coupon.Code)) {
                _couponApplicationService.RemoveCoupon(coupon.Code);
            }

            return RedirectToAction("Index", "ShoppingCart");
        }
    }
}
