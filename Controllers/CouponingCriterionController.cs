using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services.Couponing;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Couponing")]
    [ValidateInput(false), Admin]
    public class CouponingCriterionController :Controller {

        private readonly IAuthorizer _authorizer;
        private readonly ICouponApplicationService _couponApplicationService;
        private readonly ICouponRepositoryService _couponRepositoryService;

        public CouponingCriterionController(
            IAuthorizer authorizer,
            ICouponApplicationService couponApplicationService,
            ICouponRepositoryService couponRepositoryService) {

            _authorizer = authorizer;
            _couponApplicationService = couponApplicationService;
            _couponRepositoryService = couponRepositoryService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Show a list of all criteria that may be added to the coupon
        /// </summary>
        /// <param name="id">Id of the CouponRecord</param>
        /// <returns></returns>
        public ActionResult Add(int id) {
            if (!_authorizer.Authorize(
                CouponingPermissions.ManageCoupons,
                T("Not authorized to manage coupons"))) {
                return new HttpUnauthorizedResult();
            }

            var coupon = _couponRepositoryService.Get(id);
            if (coupon == null) {
                return new HttpNotFoundResult();
            }

            return View();
        }

        public ActionResult Edit(int id, string category, string type, int criterionId = -1) {
            if (!_authorizer.Authorize(
                CouponingPermissions.ManageCoupons,
                T("Not authorized to manage coupons"))) {
                return new HttpUnauthorizedResult();
            }

            return View();
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(
            int id,
            string category,
            string type,
            [DefaultValue(-1)] int criterionId,
            FormCollection formCollection) {
            if (!_authorizer.Authorize(
                CouponingPermissions.ManageCoupons,
                T("Not authorized to manage coupons"))) {
                return new HttpUnauthorizedResult();
            }

            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id, int criterionId) {
            if (!_authorizer.Authorize(
                CouponingPermissions.ManageCoupons,
                T("Not authorized to manage coupons"))) {
                return new HttpUnauthorizedResult();
            }

            // redirect to editor for the coupon
            return RedirectToAction("Edit", "CouponingAdmin", new { id = id });
        }
    }
}
