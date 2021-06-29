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
using Orchard.Forms.Services;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Orchard;
using System.Globalization;

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
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICouponCriteriaManagementService _couponCriteriaManagementService;

        private readonly Lazy<CultureInfo> _cultureInfo;

        public CouponingAdminController(
            IShapeFactory shapeFactory,
            IAuthorizer authorizer,
            ISiteService siteService,
            ICouponRepositoryService couponRepositoryService,
            ITransactionManager transactionManager,
            INotifier notifier,
            IWorkContextAccessor workContextAccessor,
            ICouponCriteriaManagementService couponCriteriaManagementService) {

            _authorizer = authorizer;
            _siteService = siteService;
            _couponRepositoryService = couponRepositoryService;
            _transactionManager = transactionManager;
            _notifier = notifier;
            _workContextAccessor = workContextAccessor;
            _couponCriteriaManagementService = couponCriteriaManagementService;

            _shapeFactory = shapeFactory;

            _cultureInfo = new Lazy<CultureInfo>(() =>
                CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));

            T = NullLocalizer.Instance;
        }

        public Localizer T;
        dynamic _shapeFactory;

        [HttpGet]
        public ActionResult Index(FilterOptions filterOptions, PagerParameters pagerParameters) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }

            var items = _couponRepositoryService
                .Query();

            if (!string.IsNullOrWhiteSpace(filterOptions.Name)) {
                items = items
                    .Where(c => c.Name.ToLower().Contains(filterOptions.Name.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filterOptions.Code)) {
                items = items
                    .Where(c => c.Code.ToLower().Contains(filterOptions.Code));
            }

            int specificPriority = 0;
            int fromPriority = 0;
            int toPriority = 0;


            switch (filterOptions.SelectedPriority) {
                case TypePriority.Equals:
                if (!string.IsNullOrWhiteSpace(filterOptions.Priority) && int.TryParse(filterOptions.Priority, out specificPriority)) {
                    items = items
                        .Where(c => c.Priority == specificPriority);
                }
                break;
                case TypePriority.FromTo:
                // se from valido e to vuoto where priority > from
                // se to è valido e from vuoto where priority < to
                // se from è valido e to è valido where priority > from && priority < to

                if (!string.IsNullOrWhiteSpace(filterOptions.PriorityFrom) && int.TryParse(filterOptions.PriorityFrom, out fromPriority)
                    && string.IsNullOrWhiteSpace(filterOptions.PriorityTo)) {
                    items = items
                        .Where(c => c.Priority >= fromPriority);
                }
                if (!string.IsNullOrWhiteSpace(filterOptions.PriorityTo) && int.TryParse(filterOptions.PriorityTo, out toPriority)
                    && string.IsNullOrWhiteSpace(filterOptions.PriorityFrom)) {
                    items = items
                        .Where(c => c.Priority <= toPriority);
                }

                if (!string.IsNullOrWhiteSpace(filterOptions.PriorityFrom) && !string.IsNullOrWhiteSpace(filterOptions.PriorityTo)) {
                    if (fromPriority <= toPriority) {
                        items = items
                          .Where(c => c.Priority >= fromPriority && c.Priority <= toPriority);
                    }
                    else {
                        //error
                        _notifier.Add(NotifyType.Error, T("Highest priority must be greater to lowest priority."));
                    }
                }
                break;
                case TypePriority.From:
                if (!string.IsNullOrWhiteSpace(filterOptions.PriorityFrom) && int.TryParse(filterOptions.PriorityFrom, out fromPriority)) {
                    items = items
                        .Where(c => c.Priority >= fromPriority);
                }
                break;
                case TypePriority.To:
                if (!string.IsNullOrWhiteSpace(filterOptions.PriorityTo) && int.TryParse(filterOptions.PriorityTo, out toPriority)) {
                    items = items
                        .Where(c => c.Priority <= toPriority);
                }
                break;
            }

            if (filterOptions.State != FiterOptionState.All) {
                bool filterState = false;
                if (filterOptions.State == FiterOptionState.Published) {
                    filterState = true;
                }
                items = items
                    .Where(c => c.Published == filterState);
            }

            if (filterOptions.ValueType != FiterOptionValueType.All) {
                items = items
                    .Where(c => c.CouponType.ToString() == filterOptions.ValueType.ToString());
            }

            switch (filterOptions.OrderBy) {
                case FilterOrderBy.Code:
                if (filterOptions.Descending) {
                    items = items
                        .OrderByDescending(c => c.Code);
                }
                else {
                    items = items
                        .OrderBy(c => c.Code);
                }
                break;
                case FilterOrderBy.Name:
                if (filterOptions.Descending) {
                    items = items
                        .OrderByDescending(c => c.Name);
                }
                else {
                    items = items
                     .OrderBy(c => c.Name);
                }
                break;
                case FilterOrderBy.Priority:
                if (filterOptions.Descending) {
                    items = items
                        .OrderByDescending(c => c.Priority);
                }
                else {
                    items = items
                        .OrderBy(c => c.Priority);
                }
                break;
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = _shapeFactory.Pager(pager)
                .TotalItemCount(items.Count());

            var itemsCoupon = items
              .Paginate(pager.GetStartIndex(), pager.PageSize)
              .ToCoupon(_cultureInfo.Value);

            dynamic viewModel = _shapeFactory.ViewModel()
                .Coupons(itemsCoupon)
                .Pager(pagerShape)
                .FilterOptions(filterOptions);
            //TODO: Add bulk actions: None, Delete Selected, Delete All, Export...

            return View((object)viewModel);
        }

        [HttpPost, ActionName("Index")]
        [Orchard.Mvc.FormValueRequired("submit.Filter")]
        public ActionResult IndexPost(FilterOptions filterOptions) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var routeValues = ControllerContext.RouteData.Values;
            if (string.IsNullOrWhiteSpace(filterOptions.Name)) {
                routeValues.Remove("FilterOptions.Name");
            }
            else {
                routeValues["FilterOptions.Name"] = filterOptions.Name;
            }
            if (string.IsNullOrWhiteSpace(filterOptions.Code)) {
                routeValues.Remove("FilterOptions.Code");
            }
            else {
                routeValues["FilterOptions.Code"] = filterOptions.Code;
            }
            if (string.IsNullOrWhiteSpace(filterOptions.Code)) {
                routeValues.Remove("FilterOptions.Code");
            }
            else {
                routeValues["FilterOptions.Code"] = filterOptions.Code;
            }
            if (string.IsNullOrWhiteSpace(filterOptions.Priority)) {
                routeValues.Remove("FilterOptions.Priority");
            }
            else {
                routeValues["FilterOptions.Priority"] = filterOptions.Priority;
            }
            if (string.IsNullOrWhiteSpace(filterOptions.PriorityFrom)) {
                routeValues.Remove("FilterOptions.PriorityFrom");
            }
            else {
                routeValues["FilterOptions.PriorityFrom"] = filterOptions.PriorityFrom;
            }
            if (string.IsNullOrWhiteSpace(filterOptions.PriorityTo)) {
                routeValues.Remove("FilterOptions.PriorityTo");
            }
            else {
                routeValues["FilterOptions.PriorityTo"] = filterOptions.PriorityTo;
            }
            routeValues["FilterOptions.SelectedPriority"] = filterOptions.SelectedPriority;

            routeValues["FilterOptions.ValueType"] = filterOptions.ValueType;
            routeValues["FilterOptions.State"] = filterOptions.State;
            routeValues["FilterOptions.OrderBy"] = filterOptions.OrderBy;
            routeValues["FilterOptions.Descending"] = filterOptions.Descending;

            return RedirectToAction("Index", routeValues);
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
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var model = new Coupon();
            if (!TryUpdateModel(model, null, null, new[] { "Id" })) {
                _transactionManager.Cancel();
                return View(model);
            }
            decimal value;
            if (!decimal.TryParse(model.Value, NumberStyles.Any, _cultureInfo.Value, out value)) {
                _transactionManager.Cancel();
                AddModelError("Value", T("{0} is an invalid number", T(model.Value)));
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
            if (!_couponRepositoryService.Validate(model)) {
                _transactionManager.Cancel();
                AddModelError("CouponingRepositoryError", T("The coupon is not valid."));
                return View(model);
            }
            _notifier.Add(NotifyType.Information, T("The coupon has been created."));
            return RedirectToAction("Edit", new { id = model.Id });
        }

        [HttpGet]
        public ActionResult Edit(int id) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var coupon = _couponRepositoryService.Get(id);
            if (coupon == null || coupon.Record == null) {
                return HttpNotFound();
            }
            return EditView(coupon);
        }

        [HttpPost, ActionName("Edit")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditPost(int id) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }
            var model = _couponRepositoryService.Get(id);
            if (model == null || model.Record == null) {
                return HttpNotFound();
            }
            if (!TryUpdateModel(model, null, null, new[] { "Id" })) {
                _transactionManager.Cancel();
                return EditView(model);
            }
            decimal value;
            if (!decimal.TryParse(model.Value, NumberStyles.Any, _cultureInfo.Value, out value)) {
                _transactionManager.Cancel();
                AddModelError("Value", T("{0} is an invalid number", T(model.Value)));
                return EditView(model);
            }
            try {
                _couponRepositoryService.UpdateRecord(model);
            }
            catch (Exception ex) {
                _transactionManager.Cancel();
                AddModelError("CouponingRepositoryError", ex.Message);
                return EditView(model);
            }

            if (!_couponRepositoryService.Validate(model)) {
                _transactionManager.Cancel();
                AddModelError("CouponingRepositoryError", T("The coupon is not valid."));
                return EditView(model);
            }

            _notifier.Add(NotifyType.Information, T("The coupon has been updated."));
            return RedirectToAction("Edit", new { id = model.Id });
        }

        private ActionResult EditView(Coupon coupon, bool retry = true) {
            if (coupon == null || coupon.Record == null) {
                return HttpNotFound();
            }
            try {
                // This portion may fail when we have canceled a transaction for an error.
                // That would cause an exception as NHibernate tries to fetch lazy information
                // while the transaction has already been canceled.

                // The following information is in the CouponRecord. Here we are 
                // "translating" it to the vms.
                // populate the vm "summaries" for the criteria
                foreach (var crit in coupon.Record.ApplicabilityCriteria) {
                    var descriptor = _couponCriteriaManagementService
                        .GetCriterion(crit.Category, crit.Type);
                    if (descriptor != null) {
                        coupon.ApplicabilityCriteria.Add(
                            CriterionToEntry(crit, descriptor));
                    }
                }
                // populate the vm "summaries" for the line conditions
                foreach (var crit in coupon.Record.LineCriteria) {
                    var descriptor = _couponCriteriaManagementService
                        .GetLineCriterion(crit.Category, crit.Type);
                    if (descriptor != null) {
                        coupon.LineCriteria.Add(
                            CriterionToEntry(crit, descriptor));
                    }
                }
            }
            catch (Exception) {
                if (retry) {
                    // fetch the record again and try to display it.
                    var dbCoupon = _couponRepositoryService.Get(coupon.Id);
                    // copy edited information from the vm we started from so that it
                    // carries over into the UI
                    dbCoupon.Value = coupon.Value;
                    dbCoupon.Name = coupon.Name;
                    dbCoupon.Code = coupon.Code;
                    dbCoupon.CouponType = coupon.CouponType;
                    dbCoupon.Published = coupon.Published;
                    // don't try to fetch everything again
                    return EditView(dbCoupon, false);
                }
            }

            return View(coupon);
        }

        private CouponApplicabilityCriterionEntry CriterionToEntry(
            CouponCriterionBaseRecord criterion, CouponCriterionDescriptor descriptor) {
            return new CouponApplicabilityCriterionEntry {
                Category = descriptor.Category,
                Type = descriptor.Type,
                CriterionRecordId = criterion.Id,
                DisplayText = string.IsNullOrWhiteSpace(criterion.Description)
                    ? descriptor.Display(new CouponContext {
                        State = FormParametersHelper.ToDynamic(criterion.State)
                    }).Text
                    : criterion.Description,
                IsAvailableForConfiguration = descriptor.IsAvailableForConfiguration,
                IsAvailableForProcessing = descriptor.IsAvailableForProcessing
            };
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
