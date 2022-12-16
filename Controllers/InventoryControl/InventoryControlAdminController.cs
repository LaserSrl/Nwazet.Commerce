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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers.InventoryControl {
    [OrchardFeature("Nwazet.InventoryControl")]
    [Admin]
    public class InventoryControlAdminController : Controller {
        // The fact this controller is currently decorated as [Admin] means it will
        // respond with the standard "Unauthorized" webpage to any request from user
        // who lack the permission to access the backend.
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

            if (quantity < 1) {
                return Unauthorized(T("Quantity not valid: must be greater than 0."));
            }
            var product = _contentManager.Get<ProductPart>(id);
            if (product == null) {
                return Unauthorized(T("Product not valid."));
            }
            if (!_authorizer.Authorize(
                CommercePermissions.ManageProducts, product, T("Not authorized to manage products"))) {
                return Unauthorized(T("Not authorized to manage products"));
            }
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

            if (quantity < 1) {
                return Unauthorized(T("Quantity not valid: must be greater than 0."));
            }
            var product = _contentManager.Get<ProductPart>(id);
            if (product == null) {
                return Unauthorized(T("Product not valid."));
            }
            if (!_authorizer.Authorize(
                CommercePermissions.ManageProducts, product, T("Not authorized to manage products"))) {
                return Unauthorized(T("Not authorized to manage products"));
            }
            // TODO: bundles, combinations...
            _productInventoryService.UpdateInventory(product, quantity);
            return new JsonResult {
                Data = new {
                    Sku = product.Sku,
                    Inventory = _productInventoryService.GetInventory(product)
                }
            };
        }

        private JsonResult Unauthorized(LocalizedString msg) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            // prevent IIS 7.0 classic mode from handling the 401 itself
            Response.SuppressFormsAuthenticationRedirect = true;
            return new JsonResult {
                Data = new { Message = msg.Text }
            };
        } 
    }
}
