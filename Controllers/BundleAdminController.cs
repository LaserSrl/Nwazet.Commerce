using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Inventory;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Bundles")]
    [Admin]
    public class BundleAdminController : Controller {
        private readonly IBundleService _bundleService;
        private readonly IBundleAutocompleteService _bundleAutocompleteService;
        private readonly IContentManager _contentManager;
        private readonly IProductInventoryService _productInventoryService;

        public BundleAdminController(
            IBundleService bundleService,
            IBundleAutocompleteService bundleAutocompleteService,
            IContentManager contentManager,
            IProductInventoryService productInventoryService) {

            _bundleService = bundleService;
            _bundleAutocompleteService = bundleAutocompleteService;
            _contentManager = contentManager;
            _productInventoryService = productInventoryService;
        }

        [HttpPost]
        public ActionResult SearchProduct(int contentItemId,string searchText,List<int> excludedProductIds) {
           var model =  _bundleAutocompleteService.GetProducts(contentItemId, searchText, excludedProductIds);
           return Json(model);
         }
    }
}
