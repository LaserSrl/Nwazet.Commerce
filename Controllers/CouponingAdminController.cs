using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Nwazet.Commerce.Extensions;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Linq;
using System.Web.Mvc;


namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Couponing")]
    [Admin]
    [ValidateInput(false)]
    public class CouponingAdminController : Controller, IUpdateModel {

        private readonly IAuthorizer _authorizer;
        private readonly ISiteService _siteService;
        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly ITransactionManager _transactionManager;
        private readonly INotifier _notifier;

        public CouponingAdminController(
            IShapeFactory shapeFactory,
            IAuthorizer authorizer,
            ISiteService siteService,
            ICouponRepositoryService couponRepositoryService,
            ITransactionManager transactionManager,
            INotifier notifier) {

            _authorizer = authorizer;
            _siteService = siteService;
            _couponRepositoryService = couponRepositoryService;
            _transactionManager = transactionManager;
            _notifier = notifier;

            _shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public Localizer T;
        dynamic _shapeFactory;

        [HttpGet]
        public ActionResult Index(FilterOptions filterOptions, PagerParameters pagerParameters) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }


            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = _shapeFactory.Pager(pager)
                .TotalItemCount(_couponRepositoryService.GetCoupons().Count());

            var items = _couponRepositoryService
                .GetCoupons().OrderBy(x => x.Code).Paginate(pager.GetStartIndex(), pager.PageSize).ToCoupon();

            dynamic viewModel = _shapeFactory.ViewModel()
                .Coupons(items)
                .Pager(pagerShape);
            //TODO: Add bulk actions: None, Delete Selected, Delete All, Export...

            return View((object)viewModel);
        }

        [HttpPost, ActionName("Index")]
        [Orchard.Mvc.FormValueRequired("submit.Filter")]
        public ActionResult IndexPost(FilterOptions filterOptions, PagerParameters pagerParameters) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            return null;
        }

        [HttpGet]
        public ActionResult Create() {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var model = new Coupon();
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreatePost() {
            //TODO: read the record from the DB and update it with the Model
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var model = new Coupon();
            if (!TryUpdateModel(model, null, null, new[] { "Id" })) {
                _transactionManager.Cancel();
                return View(model);
            }
            try {
                model.Id = _couponRepositoryService.CreateRecord(model);
            }
            catch (Exception ex) {
                _transactionManager.Cancel();
                AddModelError("CouponingRepositoryError", ex.Message);
                return View(model);
            }
            _notifier.Add(NotifyType.Information, T("The coupon has been created."));
            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpGet]
        public ActionResult Edit(int id) {
            //TODO: read the record from the DB
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var model = _couponRepositoryService.Get(id);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditPost(int id) {
            //TODO: read the record from the DB and update it with the Model
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var model = _couponRepositoryService.Get(id);
            if (!TryUpdateModel(model, null, null, new[] { "Id" })) {
                _transactionManager.Cancel();
                return View(model);
            }
            try {
                _couponRepositoryService.UpdateRecord(model);
            }
            catch (Exception ex) {
                _transactionManager.Cancel();
                AddModelError("CouponingRepositoryError", ex.Message);
                return View(model);
            }
            _notifier.Add(NotifyType.Information, T("The coupon has been updated."));
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            try {
                _couponRepositoryService.DeleteRecord(id);
            }
            catch (Exception ex) {
                _transactionManager.Cancel();
                AddModelError("CouponingRepositoryError", ex.Message);
            }
            _notifier.Add(NotifyType.Information, T("The coupon has been permanently deleted."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }


        #region IUpdateModel implementation
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, string errorMessage) {
            ModelState.AddModelError(key, errorMessage);
        }
        #endregion
    }
}
