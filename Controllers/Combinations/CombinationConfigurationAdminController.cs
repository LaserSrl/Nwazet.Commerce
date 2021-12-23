using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.Settings.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CorePermissions = Orchard.Core.Contents.Permissions;

namespace Nwazet.Commerce.Controllers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    [Admin]
    public class CombinationConfigurationAdminController : ContentControllerBase, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IProductCombinationService _productCombinationService;
        private readonly Lazy<IEnumerable<ICombinationDetailProvider>> _combinationDetailProviders;
        
        public CombinationConfigurationAdminController(
             IOrchardServices orchardServices,
             IContentDefinitionManager contentDefinitionManager,
             ITransactionManager transactionManager,
             IProductCombinationService productCombinationService,
             Lazy<IEnumerable<ICombinationDetailProvider>> combinationDetailProviders) : base(orchardServices.ContentManager) {
            Services = orchardServices;
            _contentManager = orchardServices.ContentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            _productCombinationService = productCombinationService;
            _combinationDetailProviders = combinationDetailProviders;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; private set; }

        // Create GET
        // the content is created in this controller 
        // and the LocalizedCombinationPartDriver.cs driver associates the combination
        public ActionResult Create(int id) {
            if (!Services.Authorizer.Authorize(CorePermissions.CreateContent, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            // in case of creation / new translation of a combination
            var combinationContainerPart = _contentManager.Get(id, VersionOptions.Latest)
                  ?.As<CombinationContainerPart>();

            if (combinationContainerPart == null)
                return HttpNotFound();

            // created a new content
            var contentType = _productCombinationService.GetCombinationContentType(combinationContainerPart);
            var newItem = _contentManager.New(contentType);
            var combinationPart = newItem.As<CombinationPart>();
            // assigned the container from where it starts
            combinationPart.CombinationContainerPartField.Value = combinationContainerPart;
            foreach (var provider in DetailProviders) {
                provider.Synchronize(combinationContainerPart, combinationPart);
            }

            var model = _contentManager.BuildEditor(combinationPart);
            return View(model);
        }

        private IEnumerable<ICombinationDetailProvider> DetailProviders {
            // We don't set up an infrastructure like the IContentHandler.Invoke
            // to safely use these providers because it's probably enough to just do
            // it as a method here because these providers currently aren't used
            // elsewhere.
            get { return _combinationDetailProviders.Value; }
        }

        [HttpPost, ActionName("Create")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreateCombinationPost(string id, string contentType, string returnUrl) {
            return CreateCombinationPost(id, contentType, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }
        // Create POST
        private ActionResult CreateCombinationPost(string id, string contentType, string returnUrl, Action<ContentItem> conditionallyPublish) {
            int contentId;
            if (!int.TryParse(id, out contentId)) {
                return new HttpUnauthorizedResult();
            }
            var content = _contentManager.Get(contentId, VersionOptions.Latest);
            var combinationContainerPart = content
                ?.As<CombinationContainerPart>();

            if (combinationContainerPart == null) {
                return new HttpUnauthorizedResult();
            }

            // check if it is the same content type
            var actualContentType = _productCombinationService.GetCombinationContentType(combinationContainerPart);
            if (contentType != actualContentType) {
                return new HttpUnauthorizedResult();
            }
            var contentItem = _contentManager.New(actualContentType);

            if (!Services.Authorizer.Authorize(CorePermissions.EditContent, contentItem, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            var combinationPart = contentItem
                   .As<CombinationPart>();
            // assigned the container from where it starts
            combinationPart.CombinationContainerPartField.Value = combinationContainerPart;

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            // determines whether to publish or save
            conditionallyPublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));
            if (!string.IsNullOrEmpty(returnUrl)) {
                return this.RedirectLocal(returnUrl);
            }
            var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        [HttpPost, ActionName("Create")]
        [Orchard.Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateCombinationPublishPost(string id, string contentType, string returnUrl) {
            return CreateCombinationPost(id, contentType, returnUrl, contentItem => 
                Services.ContentManager.Publish(contentItem)
            );
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
