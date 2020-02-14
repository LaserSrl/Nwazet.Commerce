using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Mvc.Extensions;
using System.Web.Routing;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;
using Nwazet.Commerce.Extensions;
using Orchard.Data;
using Orchard.ContentManagement.MetaData;
using Nwazet.Commerce.ViewModels;
using Orchard.Core.Title.Models;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Commerce")]
    [ValidateInput(false), Admin]
    public class ProductAdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _wca;
        private readonly IOrchardServices _orchardServices;
        private readonly IProductInventoryService _productInventoryService;
        private readonly IProductService _productService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public ProductAdminController(
            IOrchardServices services,
            IContentManager contentManager,
            ISiteService siteService,
            IWorkContextAccessor wca,
            IShapeFactory shapeFactory,
            IOrchardServices orchardServices,
            IProductInventoryService productInventoryService,
            IProductService productService,
            IAuthorizer authorizer,
            INotifier notifier,
            ITransactionManager transactionManager,
            IContentDefinitionManager contentDefinitionManager) {

            Services = services;
            _contentManager = contentManager;
            _siteService = siteService;
            _wca = wca;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            _orchardServices = orchardServices;
            _productInventoryService = productInventoryService;
            _productService = productService;
            _authorizer = authorizer;
            _notifier = notifier;
            _transactionManager = transactionManager;
            _contentDefinitionManager = contentDefinitionManager;

            _allowedProductType = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedProductTypes);
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        #region Create
        [HttpGet]
        public ActionResult CreateProduct(string id) {
            if (AllowedProductTypes == null) {
                return new HttpUnauthorizedResult(ProductUtilities.Default401ProductMessage);
            }

            if (!AllowedProductTypes.Any()) { //nothing to do
                return RedirectToAction("List");
            }

            if (!string.IsNullOrWhiteSpace(id)) { //specific type requested
                var typeDefinition = AllowedProductTypes.FirstOrDefault(ctd => ctd.Name == id);
                if (typeDefinition != null) {
                    return CreateProduct(typeDefinition);
                }
            }

            if (AllowedProductTypes.Count() == 1) {
                return CreateProduct(AllowedProductTypes.FirstOrDefault());
            }
            else {
                return CreatableProductsList();
            }
        }
        private ActionResult CreateProduct(ContentTypeDefinition typeDefinition)
        {
            if (AllowedProductTypes == null)
            {
                return new HttpUnauthorizedResult(ProductUtilities.Default401ProductMessage);
            }
            if (!AllowedProductTypes.Any(ty => ty.Name == typeDefinition.Name))
            {
                return new HttpUnauthorizedResult(ProductUtilities.SpecificProduct401Message(typeDefinition.DisplayName));
            }
            if (!typeDefinition.Parts.Any(pa => pa.PartDefinition.Name == ProductPart.PartName))
            {
                AddModelError("", T("The requested type \"{0}\" is not a Product type.", typeDefinition.DisplayName));
                return RedirectToAction("List");
            }
            //We should have filtered out the cases where we cannot or should not be creating the new item here
            var productItem = _contentManager.New(typeDefinition.Name);
            var model = _contentManager.BuildEditor(productItem);
            return View(model);
        }
        private ActionResult CreatableProductsList()
        {
            if (AllowedProductTypes == null)
            {
                return new HttpUnauthorizedResult(ProductUtilities.Default401ProductMessage);
            }
            //This will be like the AdminController from Orchard.Core.Contents
            var viewModel = Shape.ViewModel(ProductTypes: AllowedProductTypes);

            return View("CreatableTypeList", viewModel);
        }

        [HttpPost, ActionName("CreateProduct")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreateProductPost(string id, string returnUrl)
        {
            return CreateProductPost(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                {
                    _contentManager.Publish(contentItem);
                }
            });
        }

        [HttpPost, ActionName("CreateProduct")]
        [Orchard.Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishProductPost(string id, string returnUrl)
        {
            var dummyContent = _contentManager.New(id);

            if (!_authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, dummyContent, ProductUtilities.Creation401ProductMessage))
                return new HttpUnauthorizedResult();

            return CreateProductPost(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult CreateProductPost(string typeName, string returnUrl, Action<ContentItem> conditionallyPublish)
        {
            return ExecuteProductPost(new ProductExecutionContext
            {
                ProductItem = _contentManager.New(typeName),
                Message = ProductUtilities.Creation401ProductMessage,
                AdditionalPermissions = new Permission[] { Orchard.Core.Contents.Permissions.EditContent },
                ExecutionAction = item => {
                    _contentManager.Create(item, VersionOptions.Draft);

                    var model = _contentManager.UpdateEditor(item, this);

                    if (!ModelState.IsValid)
                    {
                        _transactionManager.Cancel();
                        return View(model);
                    }

                    conditionallyPublish(item);

                    _notifier.Information(string.IsNullOrWhiteSpace(item.TypeDefinition.DisplayName)
                        ? T("Your content has been created.")
                        : T("Your {0} has been created.", item.TypeDefinition.DisplayName));

                    return this.RedirectLocal(returnUrl, () =>
                        RedirectToAction("EditProduct", new RouteValueDictionary { { "Id", item.Id } }));
                }
            });
        }
        private ActionResult ExecuteProductPost(
           ProductExecutionContext context)
        {
            var productItem = context.ProductItem;
            if (productItem == null)
                return HttpNotFound();

            #region Authorize
            if (AllowedProductTypes == null)
            {
                return new HttpUnauthorizedResult(ProductUtilities.Default401ProductMessage);
            }
            var typeName = productItem.ContentType;
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName);
            if (!typeDefinition.Parts.Any(pa => pa.PartDefinition.Name == ProductPart.PartName))
            {
                AddModelError("", T("The requested type \"{0}\" is not a Product type.", typeDefinition.DisplayName));
                return RedirectToAction("List");
            }
            typeDefinition = AllowedProductTypes.FirstOrDefault(ctd => ctd.Name == typeName);
            if (typeDefinition == null)
            {
                return new HttpUnauthorizedResult(ProductUtilities.SpecificProduct401Message(typeName));
            }
            
            foreach (var permission in context.AdditionalPermissions)
            {
                if (!_authorizer.Authorize(permission, productItem, context.Message))
                    return new HttpUnauthorizedResult();
            }
            #endregion

            return context.ExecutionAction(productItem);
        }
        #endregion
        
        public ActionResult List(ListProductsViewModel model, PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(CommercePermissions.ManageProducts, null, T("Not authorized to manage products"))) 
                return new HttpUnauthorizedResult();
            
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var query = _contentManager.Query<ProductPart, ProductPartVersionRecord>(VersionOptions.Latest);

            if (!string.IsNullOrWhiteSpace(model.Options.Title)) {
                query = query
                    .Join<TitlePartRecord>()
                    .Where(o => o.Title.Contains(model.Options.Title))
                    .Join<ProductPartVersionRecord>();
            }

            if (!string.IsNullOrWhiteSpace(model.Options.Sku)) {
                query = query
                    .Where(o => o.Sku.Contains(model.Options.Sku));
            }

            switch (model.Options.OrderBy) {
                case ContentsProduct.Modified:
                    query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsProduct.Published:
                    query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsProduct.Created:
                    query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .AllowedProductTypes(AllowedProductTypes.ToList());

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        [HttpPost]
        [ActionName("List")]
        [Orchard.Mvc.FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPost(ContentProductOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.OrderBy"] = options.OrderBy;

                if (String.IsNullOrWhiteSpace(options.Title)) {
                    routeValues.Remove("Options.Title");
                }
                else {
                    routeValues["Options.Title"] = options.Title;
                }
                if (String.IsNullOrWhiteSpace(options.Sku)) {
                    routeValues.Remove("Options.Sku");
                }
                else {
                    routeValues["Options.Sku"] = options.Sku;
                }
            }

            return RedirectToAction("List", routeValues);
        }

        [HttpPost]
        public ActionResult RemoveOne(int id) {
            if (!_orchardServices.Authorizer.Authorize(CommercePermissions.ManageProducts, null, T("Not authorized to manage products")))
                return new HttpUnauthorizedResult();            

            var product = _contentManager.Get<ProductPart>(id);
            _productInventoryService.UpdateInventory(product, -1);
            Dictionary<string, int> newInventory;
            IBundleService bundleService;
            if (_wca.GetContext().TryResolve(out bundleService)) {
                var affectedBundles = _contentManager.Query<BundlePart, BundlePartRecord>()
                    .Where(b => b.Products.Any(p => p.ContentItemRecord.Id == product.Id))
                    .WithQueryHints(new QueryHints().ExpandParts<ProductPart>())
                    .List();
                newInventory = affectedBundles.ToDictionary(
                    b => b.As<ProductPart>().Sku,
                    b => bundleService.GetProductQuantitiesFor(b).Min(p => _productInventoryService.GetInventory(p.Product) / p.Quantity));
            } else {
                newInventory = new Dictionary<string, int>(1);
            }
            newInventory.Add(product.Sku, _productInventoryService.GetInventory(product));
            return new JsonResult {
                Data = newInventory
            };
        }

        [HttpPost]
        public ActionResult AddOne(int id) {
            if (!_orchardServices.Authorizer.Authorize(CommercePermissions.ManageProducts, null, T("Not authorized to manage products")))
                return new HttpUnauthorizedResult();

            var product = _contentManager.Get<ProductPart>(id);
            _productInventoryService.UpdateInventory(product, 1);
            Dictionary<string, int> newInventory;
            IBundleService bundleService;
            if (_wca.GetContext().TryResolve(out bundleService)) {
                var affectedBundles = _contentManager.Query<BundlePart, BundlePartRecord>()
                    .Where(b => b.Products.Any(p => p.ContentItemRecord.Id == product.Id))
                    .WithQueryHints(new QueryHints().ExpandParts<ProductPart>())
                    .List();
                newInventory = affectedBundles.ToDictionary(
                    b => b.As<ProductPart>().Sku,
                    b => bundleService.GetProductQuantitiesFor(b).Min(p => _productInventoryService.GetInventory(p.Product) / p.Quantity));
            }
            else {
                newInventory = new Dictionary<string, int>(1);
            }
            newInventory.Add(product.Sku, _productInventoryService.GetInventory(product));
            return new JsonResult {
                Data = newInventory
            };
        }

        private Lazy<IEnumerable<ContentTypeDefinition>> _allowedProductType;
        private IEnumerable<ContentTypeDefinition> AllowedProductTypes {
            get { return _allowedProductType.Value; }
        }

        /// <summary>
        /// This method gets all the product types the current user is allowed to manage.
        /// </summary>
        /// <returns>Returns the types the user is allwoed to manage. Returns null if the user lacks the correct 
        /// permissions to be invoking these actions.</returns>
        private IEnumerable<ContentTypeDefinition> GetAllowedProductTypes() {
            var allowedTypes = _productService.GetProductTypes();
            if (!allowedTypes.Any() || //no dynamic permissions
                !_authorizer.Authorize(CommercePermissions.ManageProducts)) {

                return null;
            }

            return allowedTypes;
        }

        #region IUpdateModel implementation
        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, string errorMessage) {
            ModelState.AddModelError(key, errorMessage);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
    }
}