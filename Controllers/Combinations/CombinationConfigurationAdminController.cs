using Nwazet.Commerce.Models;
using Nwazet.Commerce.Settings.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Controllers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Admin;
using System;
using System.Web.Mvc;
using CorePermissions = Orchard.Core.Contents.Permissions;

namespace Nwazet.Commerce.Controllers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    [Admin]
    public class CombinationConfigurationAdminController : ContentControllerBase, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CombinationConfigurationAdminController(
             IOrchardServices orchardServices,
             IContentDefinitionManager contentDefinitionManager) : base(orchardServices.ContentManager) {
            Services = orchardServices;
            _contentManager = orchardServices.ContentManager;
            _contentDefinitionManager = contentDefinitionManager;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; private set; }

        public ActionResult Create(int id) {
            if (!Services.Authorizer.Authorize(CorePermissions.CreateContent, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            var combinationContainerPart = _contentManager.Get(id, VersionOptions.Latest)
                  ?.As<CombinationContainerPart>();

            if (combinationContainerPart == null)
                return HttpNotFound();

            var contentType = GetCombinationContentType(combinationContainerPart);
            var newItem = _contentManager.New(contentType);
            var combinationPart = newItem.As<CombinationPart>();
            combinationPart.CombinationContainerPartField.Value = combinationContainerPart;

            var model = _contentManager.BuildEditor(combinationPart);
            return View(model);
        }

        private string GetCombinationContentType(
            CombinationContainerPart containerPart) {
            var partSettings = containerPart.TypePartDefinition
                .Settings.GetModel<CombinationContainerPartSettings>();

            return partSettings?.CombinationTypeName ?? string.Empty;
        }

        //[HttpPost, ActionName("Create")]
        //[Orchard.Mvc.FormValueRequired("submit.Save")]
        //public ActionResult CreatePOST(string id, string returnUrl) {
        //    return CreatePOST(id, returnUrl, contentItem => {
        //        _contentManager.Publish(contentItem);
        //    });
        //}
        //private ActionResult CreatePOST(string id, string returnUrl, Action<ContentItem> conditionallyPublish) {
        //    var contentItem = _contentManager.New(id);

        //    if (!Services.Authorizer.Authorize(CorePermissions.EditContent, contentItem, T("Couldn't create content")))
        //        return new HttpUnauthorizedResult();

        //    _contentManager.Create(contentItem, VersionOptions.Draft);

        //    var model = _contentManager.UpdateEditor(contentItem, this);

        //    if (!ModelState.IsValid) {
        //        _transactionManager.Cancel();
        //        return View(model);
        //    }

        //    conditionallyPublish(contentItem);

        //    Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
        //        ? T("Your content has been created.")
        //        : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));
        //    if (!string.IsNullOrEmpty(returnUrl)) {
        //        return this.RedirectLocal(returnUrl);
        //    }
        //    var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
        //    return RedirectToRoute(adminRouteValues);
        //}

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
