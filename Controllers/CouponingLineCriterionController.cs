using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Couponing")]
    [ValidateInput(false), Admin]
    public class CouponingLineCriterionController : Controller {

        private readonly IAuthorizer _authorizer;
        private readonly ICouponApplicationService _couponApplicationService;
        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IFormManager _formManager;

        public CouponingLineCriterionController(
            IAuthorizer authorizer,
            ICouponApplicationService couponApplicationService,
            ICouponRepositoryService couponRepositoryService,
            IFormManager formManager) {

            _authorizer = authorizer;
            _couponApplicationService = couponApplicationService;
            _couponRepositoryService = couponRepositoryService;
            _formManager = formManager;

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
            if (coupon == null || coupon.Record == null) {
                return HttpNotFound();
            }

            var viewModel = new CouponLineCriteriaAddViewModel {
                Id = id,
                Criteria = _couponApplicationService.DescribeLineCriteria()
            };

            return View(viewModel);
        }

        public ActionResult Edit(int id, string category, string type, int criterionId = -1) {
            if (!_authorizer.Authorize(
                CouponingPermissions.ManageCoupons,
                T("Not authorized to manage coupons"))) {
                return new HttpUnauthorizedResult();
            }

            var coupon = _couponRepositoryService.Get(id);
            if (coupon == null || coupon.Record == null) {
                return HttpNotFound();
            }

            var criterion = _couponApplicationService
                .GetLineCriterion(category, type);
            if (criterion == null) {
                return HttpNotFound();
            }
            // build the form, and let external components alter it
            var form = criterion.Form == null
                ? null
                : _formManager.Build(criterion.Form);
            string description = "";
            // bind form with existing values.
            if (criterionId != -1) {
                var critRecord = coupon.Record
                    .ApplicabilityCriteria
                    .FirstOrDefault(ac => ac.Id == criterionId);
                if (critRecord != null) {
                    description = critRecord.Description;
                    var parameters = FormParametersHelper.FromString(critRecord.State);
                    _formManager.Bind(form,
                        new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                }
            }
            var viewModel = new CouponLineCriterionEditViewModel {
                Id = id,
                Description = description,
                Criterion = criterion,
                Form = form
            };
            return View(viewModel);
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
            // Get the coupon
            var coupon = _couponRepositoryService.Get(id);
            if (coupon == null || coupon.Record == null) {
                return HttpNotFound();
            }
            // get the definition for the criterion
            var criterion = _couponApplicationService
                .GetLineCriterion(category, type);
            if (criterion == null) {
                return HttpNotFound();
            }
            var viewModel = new CouponLineCriterionEditViewModel();
            TryUpdateModel(viewModel);
            // validating form values
            _formManager.Validate(new ValidatingContext {
                FormName = criterion.Form,
                ModelState = ModelState,
                ValueProvider = ValueProvider
            });

            if (ModelState.IsValid) {
                var criterionRecord = coupon.Record
                    .LineCriteria
                    .FirstOrDefault(f => f.Id == criterionId);

                // add new criterion record if it's a newly created criterion
                if (criterionRecord == null) {
                    criterionRecord = new CouponLineCriterionRecord {
                        Category = category,
                        Type = type
                    };
                    coupon.Record.LineCriteria.Add(criterionRecord);
                }

                var dictionary = formCollection.AllKeys
                    .ToDictionary(key => key, formCollection.Get);
                // save form parameters
                criterionRecord.State = FormParametersHelper.ToString(dictionary);
                criterionRecord.Description = viewModel.Description;

                // redirect to editor for the coupon
                return RedirectToAction("Edit", "CouponingAdmin", new { id = id });
            }
            // model is invalid, display it again
            var form = _formManager.Build(criterion.Form);

            _formManager.Bind(form, formCollection);
            var vm = new CouponLineCriterionEditViewModel {
                Id = id,
                Description = viewModel.Description,
                Criterion = criterion,
                Form = form
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult Delete(int id, int criterionId) {
            if (!_authorizer.Authorize(
                CouponingPermissions.ManageCoupons,
                T("Not authorized to manage coupons"))) {
                return new HttpUnauthorizedResult();
            }

            var coupon = _couponRepositoryService.Get(id);
            if (coupon == null || coupon.Record == null) {
                return HttpNotFound();
            }
            if (criterionId >= 0) {
                var critRecord = coupon.Record
                    .LineCriteria
                    .FirstOrDefault(ac => ac.Id == criterionId);
                if (critRecord == null) {
                    // weird error condition
                    return HttpNotFound();
                }
                // actually delete
                _couponApplicationService.DeleteLineCriterion(criterionId);
            }
            // redirect to editor for the coupon
            return RedirectToAction("Edit", "CouponingAdmin", new { id = id });
        }
    }
}
