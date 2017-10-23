using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.DisplayManagement;
using Nwazet.Commerce.ViewModels;
using Nwazet.Commerce.Models;
using Orchard.Security;
using Nwazet.Commerce.Permissions;
using Orchard.Data;
using Orchard.UI.Admin;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.UI.Notify;
using Orchard.Mvc.Extensions;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Html;
using System.Web.Routing;
using Orchard.Security.Permissions;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    [Admin]
    [ValidateInput(false)]
    public class TerritoriesAdminController : Controller, IUpdateModel {

        private readonly IContentManager _contentManager;
        private readonly ITerritoriesService _territoriesService;
        private readonly ISiteService _siteService;
        private readonly IAuthorizer _authorizer;
        private readonly ITerritoriesRepositoryService _territoryRepositoryService;
        private readonly ITransactionManager _transactionManager;
        private readonly INotifier _notifier;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TerritoriesAdminController(
            IContentManager contentManager,
            ITerritoriesService territoriesService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizer authorizer,
            ITerritoriesRepositoryService territoryRepositoryService,
            ITransactionManager transactionManager,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager) {

            _contentManager = contentManager;
            _territoriesService = territoriesService;
            _siteService = siteService;
            _authorizer = authorizer;
            _territoryRepositoryService = territoryRepositoryService;
            _transactionManager = transactionManager;
            _notifier = notifier;
            _contentDefinitionManager = contentDefinitionManager;

            _shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;

            _allowedHierarchyTypes = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedHierarchyTypes);

            default401HierarchyMessage = T("Not authorized to manage hierarchies.").Text;
            creation401HierarchyMessage = T("Couldn't create hierarchy");
            edit401HierarchyMessage = T("Couldn't edit hierarchy");
            delete401HierarchyMessage = T("Couldn't delete hierarchy");
        }

        public Localizer T;
        dynamic _shapeFactory;

        string default401HierarchyMessage; //displayed for HttpUnauthorizedResults
        LocalizedString creation401HierarchyMessage;
        LocalizedString edit401HierarchyMessage;
        LocalizedString delete401HierarchyMessage;

        #region Manage the contents for hierarches
        /// <summary>
        /// This is the entry Action to the section to manage hierarchies of territories and the territory ContentItems. 
        /// From here, users will not directly go and handle the records with the unique territory definitions.
        /// </summary>
        [HttpGet]
        public ActionResult HierarchiesIndex(PagerParameters pagerParameters) {
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(default401HierarchyMessage);
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            HierarchyAdminIndexViewModel model;
            if (AllowedHierarchyTypes.Any()) {
                var typeNames = AllowedHierarchyTypes.Select(ctd => ctd.Name).ToArray();

                var hierarchies = _territoriesService
                    .GetHierarchiesQuery(typeNames)
                    .Slice(pager.GetStartIndex(), pager.PageSize);

                var pagerShape = _shapeFactory
                    .Pager(pager)
                    .TotalItemCount(_territoriesService.GetHierarchiesQuery(typeNames).Count());

                var entries = hierarchies
                    .Select(CreateEntry)
                    .ToList();

                model = new HierarchyAdminIndexViewModel {
                    HierarchyEntries = entries,
                    AllowedHierarchyTypes = AllowedHierarchyTypes.ToList(),
                    Pager = pagerShape
                };
            } else {
                //No ContentType has been defined that contains the TerritoryHierarchyPart
                var pagerShape = _shapeFactory
                    .Pager(pager)
                    .TotalItemCount(0);

                model = new HierarchyAdminIndexViewModel {
                    HierarchyEntries = new List<HierarchyIndexEntry>(),
                    AllowedHierarchyTypes = AllowedHierarchyTypes.ToList(),
                    Pager = pagerShape
                };
                //For now we handle this by simply pointing out that the user should create types
                AddModelError("", T("There are no Hierarchy types that the user is allowed to manage."));
            }

            return View(model);
        }

        #region Creation
        [HttpGet]
        public ActionResult CreateHierarchy(string id) {
            //id is the Name of the ContentType we are trying to create. Calling that id allows us to use standard 
            //MVC routing (i.e. controller/action/id?querystring. This is especially nice for the post calls.
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(default401HierarchyMessage);
            }

            if (!AllowedHierarchyTypes.Any()) { //nothing to do
                return RedirectToAction("HierarchiesIndex");
            }

            if (!string.IsNullOrWhiteSpace(id)) { //specific type requested
                var typeDefinition = AllowedHierarchyTypes.FirstOrDefault(ctd => ctd.Name == id);
                if (typeDefinition != null) {
                    return CreateHierarchy(typeDefinition);
                }
            }
            if (AllowedHierarchyTypes.Count() == 1) {
                return CreateHierarchy(AllowedHierarchyTypes.FirstOrDefault());
            } else {
                return CreatableHierarchiesList();
            }
        }

        [HttpPost, ActionName("CreateHierarchy")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreateHierarchyPost(string id, string returnUrl) {
            return CreateHierarchyPost(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable) {
                    _contentManager.Publish(contentItem);
                }
            });
        }

        [HttpPost, ActionName("CreateHierarchy")]
        [Orchard.Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishHierarchyPost(string id, string returnUrl) {
            var dummyContent = _contentManager.New(id);

            if (!_authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, dummyContent, creation401HierarchyMessage))
                return new HttpUnauthorizedResult();

            return CreateHierarchyPost(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult CreateHierarchy(ContentTypeDefinition typeDefinition) {
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(default401HierarchyMessage);
            }
            if (!AllowedHierarchyTypes.Any(ty => ty.Name == typeDefinition.Name)) {
                return new HttpUnauthorizedResult(T("Not authorized to manage hierarchies of type \"{0}\"", typeDefinition.Name).Text);
            }
            if (!typeDefinition.Parts.Any(pa => pa.PartDefinition.Name == TerritoryHierarchyPart.PartName)) {
                AddModelError("", T("The requested type \"{0}\" is not a Hierarchy type.", typeDefinition.Name));
                return RedirectToAction("HierarchiesIndex");
            }
            //We should have filtered out the cases where we cannot or should not be creating the new item here
            var hierarchyItem = _contentManager.New(typeDefinition.Name);
            var model = _contentManager.BuildEditor(hierarchyItem);
            return View(model);
        }

        private ActionResult CreatableHierarchiesList() {
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(default401HierarchyMessage);
            }
            //This will be like the AdminController from Orchard.Core.Contents
            var viewModel = _shapeFactory.ViewModel(HierarchyTypes: AllowedHierarchyTypes);

            return View("CreatableTypeList", viewModel);
        }

        private ActionResult CreateHierarchyPost(string typeName, string returnUrl, Action<ContentItem> conditionallyPublish) {
            return ExecuteHierarchyPost(new TerritoriesAdminHierarchyExecutionContext {
                HierarchyItem = _contentManager.New(typeName),
                Message = creation401HierarchyMessage,
                AdditionalPermissions = new Permission[] { Orchard.Core.Contents.Permissions.EditContent },
                ExecutionAction = item => {
                    _contentManager.Create(item, VersionOptions.Draft);

                    var model = _contentManager.UpdateEditor(item, this);

                    if (!ModelState.IsValid) {
                        _transactionManager.Cancel();
                        return View(model);
                    }

                    conditionallyPublish(item);

                    _notifier.Information(string.IsNullOrWhiteSpace(item.TypeDefinition.DisplayName)
                        ? T("Your content has been created.")
                        : T("Your {0} has been created.", item.TypeDefinition.DisplayName));

                    return this.RedirectLocal(returnUrl, () => RedirectToAction("EditHierarchy", new RouteValueDictionary { { "Id", item.Id } }));
                }
            });
        }

        #endregion

        #region Edit
        [HttpGet]
        public ActionResult EditHierarchy(int id) {
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(default401HierarchyMessage);
            }

            var hierarchyItem = _contentManager.Get(id, VersionOptions.Latest);

            if (hierarchyItem == null)
                return HttpNotFound();

            var typeName = hierarchyItem.ContentType;
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName);
            if (!typeDefinition.Parts.Any(pa => pa.PartDefinition.Name == TerritoryHierarchyPart.PartName)) {
                AddModelError("", T("The requested type \"{0}\" is not a Hierarchy type.", typeDefinition.Name));
                return RedirectToAction("HierarchiesIndex");
            }
            typeDefinition = AllowedHierarchyTypes.FirstOrDefault(ctd => ctd.Name == typeName);

            if (typeDefinition == null) {
                return new HttpUnauthorizedResult(T("Not authorized to manage hierarchies of type \"{0}\"", typeName).Text);
            }

            if (!_authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, hierarchyItem, edit401HierarchyMessage))
                return new HttpUnauthorizedResult();

            //We should have filtered out the cases where we cannot or should not be editing the new item here
            var model = _contentManager.BuildEditor(hierarchyItem);
            return View(model);
        }

        [HttpPost, ActionName("EditHierarchy")]
        [Orchard.Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditHierarchyPost(int id, string returnUrl) {
            return EditHierarchyPost(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("EditHierarchy")]
        [Orchard.Mvc.FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishHierarchyPost(int id, string returnUrl) {
            var content = _contentManager.Get(id, VersionOptions.Latest);

            if (content == null)
                return HttpNotFound();

            if (!_authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, content, edit401HierarchyMessage))
                return new HttpUnauthorizedResult();

            return EditHierarchyPost(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult EditHierarchyPost(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            return ExecuteHierarchyPost(new TerritoriesAdminHierarchyExecutionContext {
                HierarchyItem = _contentManager.Get(id, VersionOptions.DraftRequired),
                Message = edit401HierarchyMessage,
                AdditionalPermissions = new Permission[] { Orchard.Core.Contents.Permissions.EditContent },
                ExecutionAction = item => {
                    var model = _contentManager.UpdateEditor(item, this);

                    if (!ModelState.IsValid) {
                        _transactionManager.Cancel();
                        return View(model);
                    }

                    conditionallyPublish(item);

                    _notifier.Information(string.IsNullOrWhiteSpace(item.TypeDefinition.DisplayName)
                        ? T("Your content has been saved.")
                        : T("Your {0} has been saved.", item.TypeDefinition.DisplayName));

                    return this.RedirectLocal(returnUrl, () => RedirectToAction("EditHierarchy", new RouteValueDictionary { { "Id", item.Id } }));
                }
            });
        }

        #endregion

        [HttpPost]
        public ActionResult DeleteHierarchy(int id, string returnUrl) {
            return ExecuteHierarchyPost(new TerritoriesAdminHierarchyExecutionContext {
                HierarchyItem = _contentManager.Get(id, VersionOptions.Latest),
                Message = delete401HierarchyMessage,
                AdditionalPermissions = new Permission[] { Orchard.Core.Contents.Permissions.DeleteContent },
                ExecutionAction = item => {
                    if (item != null) {
                        _contentManager.Remove(item);
                        _notifier.Information(string.IsNullOrWhiteSpace(item.TypeDefinition.DisplayName)
                            ? T("That content has been removed.")
                            : T("That {0} has been removed.", item.TypeDefinition.DisplayName));
                    }

                    return this.RedirectLocal(returnUrl, () => RedirectToAction("HierarchiesIndex"));
                }
            });
        }

        private ActionResult ExecuteHierarchyPost(
            TerritoriesAdminHierarchyExecutionContext context) {
            var hierarchyItem = context.HierarchyItem;
            if (hierarchyItem == null)
                return HttpNotFound();

            #region Authorize
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(default401HierarchyMessage);
            }
            var typeName = hierarchyItem.ContentType;
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(typeName);
            if (!typeDefinition.Parts.Any(pa => pa.PartDefinition.Name == TerritoryHierarchyPart.PartName)) {
                AddModelError("", T("The requested type \"{0}\" is not a Hierarchy type.", typeDefinition.Name));
                return RedirectToAction("HierarchiesIndex");
            }
            typeDefinition = AllowedHierarchyTypes.FirstOrDefault(ctd => ctd.Name == typeName);
            if (typeDefinition == null) {
                return new HttpUnauthorizedResult(T("Not authorized to manage hierarchies of type \"{0}\"", typeName).Text);
            }

            if (!_authorizer.Authorize(TerritoriesPermissions.ManageTerritoryHierarchies, hierarchyItem, context.Message))
                return new HttpUnauthorizedResult();

            foreach (var permission in context.AdditionalPermissions) {
                if (!_authorizer.Authorize(permission, hierarchyItem, context.Message))
                    return new HttpUnauthorizedResult();
            }
            #endregion

            return context.ExecutionAction(hierarchyItem);
        }
        #endregion

        #region Manage the unique territory records
        private readonly string[] _territoryIncludeProperties = { "Name" };
        /// <summary>
        /// This is the entry Action to the section to manage The TerritoryIntenalRecords. These are the
        /// unique records that exist behind TerritoryPartRecords, and that are used to relate "same"
        /// TerritoryParts, also across hierarchies. For example, they can uniquely match "Cyprus" from a
        /// fiscal hierarchy with "Cyprus" from a shipping hierarchy.
        /// </summary>
        [HttpGet]
        public ActionResult TerritoriesIndex(PagerParameters pagerParameters) {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = _shapeFactory.Pager(pager)
                .TotalItemCount(_territoryRepositoryService.GetTerritoriesCount());

            var items = _territoryRepositoryService.GetTerritories(pager.GetStartIndex(), pager.PageSize);

            dynamic viewModel = _shapeFactory.ViewModel()
                .Territories(items)
                .Pager(pagerShape);
            //TODO: Add bulk actions: None, Delete Selected, Delete All, Export...

            return View((object)viewModel);
        }

        [HttpGet]
        public ActionResult AddTerritoryInternal() {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            return View();
        }

        [HttpPost, ActionName("AddTerritoryInternal")]
        public ActionResult AddTerritoryInternalPost() {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            var tir = new TerritoryInternalRecord();

            if (!TryUpdateModel(tir, _territoryIncludeProperties)) {
                _transactionManager.Cancel();
                return View(tir);
            }

            try {
                _territoryRepositoryService.AddTerritory(tir);
            } catch (Exception ex) {
                AddModelError("", ex.Message);
                return View(tir);
            }

            return RedirectToAction("TerritoriesIndex");
        }

        [HttpGet]
        public ActionResult EditTerritoryInternal(int id) {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            var tir = _territoryRepositoryService.GetTerritoryInternal(id);
            if (tir == null) {
                return HttpNotFound();
            }

            return View(tir);
        }

        [HttpPost, ActionName("EditTerritoryInternal")]
        public ActionResult EditTerritoryInternalPost(int id) {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            var tir = _territoryRepositoryService.GetTerritoryInternal(id);
            if (tir == null) {
                return HttpNotFound();
            }

            if (!TryUpdateModel(tir, _territoryIncludeProperties)) {
                _transactionManager.Cancel();
                return View(tir);
            }

            try {
                _territoryRepositoryService.Update(tir);
            } catch (Exception ex) {
                AddModelError("", ex.Message);
                return View(tir);
            }

            return View(tir);
        }

        [HttpPost]
        public ActionResult DeleteTerritoryInternal(int id) {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            var tir = _territoryRepositoryService.GetTerritoryInternal(id);
            if (tir == null) {
                return HttpNotFound();
            }

            //TODO: handle TerritoryParts that may "contain" the Territory we are trying to delete

            _territoryRepositoryService.Delete(id);

            return RedirectToAction("TerritoriesIndex");
        }
        #endregion

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

        private HierarchyIndexEntry CreateEntry(TerritoryHierarchyPart part) {
            var hierarchyType = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var typeDisplayName = hierarchyType != null ?
                hierarchyType.DisplayName : T("ERROR: impossible to find hierarchy type.").Text;

            var territoryType = _contentDefinitionManager.GetTypeDefinition(part.TerritoryType);
            var territoryDisplayName = territoryType != null ?
                territoryType.DisplayName : T("ERROR: impossible to find territory type.").Text;

            return new HierarchyIndexEntry {
                Id = part.ContentItem.Id,
                DisplayText = _contentManager.GetItemMetadata(part.ContentItem).DisplayText,
                ContentItem = part.ContentItem,
                TypeDisplayName = typeDisplayName,
                IsDraft = !part.ContentItem.IsPublished(),
                TerritoriesCount = part.Territories.Count(),
                TerritoryTypeDisplayName = territoryDisplayName
            };
        }

        private Lazy<IEnumerable<ContentTypeDefinition>> _allowedHierarchyTypes;
        private IEnumerable<ContentTypeDefinition> AllowedHierarchyTypes {
            get { return _allowedHierarchyTypes.Value; }
        }

        /// <summary>
        /// This method gets all the hierarchy types the current user is allowed to manage.
        /// </summary>
        /// <returns>Returns the types the user is allwoed to manage. Returns null if the user lacks the correct 
        /// permissions to be invoking these actions.</returns>
        private IEnumerable<ContentTypeDefinition> GetAllowedHierarchyTypes() {
            var allowedTypes = _territoriesService.GetHierarchyTypes();
            if (!allowedTypes.Any() && //no dynamic permissions
                !_authorizer.Authorize(TerritoriesPermissions.ManageTerritoryHierarchies)) {

                return null;
            }

            return allowedTypes;
        }

    }
}
