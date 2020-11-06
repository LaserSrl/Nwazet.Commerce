using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services.Couponing;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Couponing")]
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
        public ActionResult Index(PagerParameters pagerParameters) {
            if (!_authorizer.Authorize(CouponingPermissions.ManageCoupons)) {
                return new HttpUnauthorizedResult();
            }


            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = _shapeFactory.Pager(pager)
                .TotalItemCount(_couponRepositoryService.GetCouponsCount());

            var items = _couponRepositoryService
                .GetCoupons(pager.GetStartIndex(), pager.PageSize);

            dynamic viewModel = _shapeFactory.ViewModel()
                .Coupons(items)
                .Pager(pagerShape);
            //TODO: Add bulk actions: None, Delete Selected, Delete All, Export...

            return View((object)viewModel);
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
