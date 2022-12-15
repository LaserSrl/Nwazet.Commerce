using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Inventory;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    [Admin]
    public class InventoryControlAdminController : Controller {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;

        public InventoryControlAdminController(
            IProductInventoryService productInventoryService,
            IAuthorizer authorizer,
            IContentManager contentManager) {

            _productInventoryService = productInventoryService;
            _authorizer = authorizer;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult RemoveInventory(int id, int quantity = 1) {
            
            var product = _contentManager.Get<ProductPart>(id);
            if (!_authorizer.Authorize(
                CommercePermissions.ManageProducts, product, T("Not authorized to manage products"))) {
                return new HttpUnauthorizedResult();
            }
            // TODO: product not null
            // TODO: quantity must be > 0
            // TODO: bundles, combinations...
            _productInventoryService.UpdateInventory(product, -quantity);
            return new JsonResult {
                Data = new { 
                    Sku = product.Sku,
                    Inventory = _productInventoryService.GetInventory(product)
                }
            };
        }

        [HttpPost]
        public ActionResult AddInventory(int id, int quantity = 1) {

            var product = _contentManager.Get<ProductPart>(id);
            if (!_authorizer.Authorize(
                CommercePermissions.ManageProducts, product, T("Not authorized to manage products"))) {
                return new HttpUnauthorizedResult();
            }
            // TODO: product not null
            // TODO: quantity must be > 0
            // TODO: bundles, combinations...
            _productInventoryService.UpdateInventory(product, quantity);
            return new JsonResult {
                Data = new {
                    Sku = product.Sku,
                    Inventory = _productInventoryService.GetInventory(product)
                }
            };
        }
    }
}
