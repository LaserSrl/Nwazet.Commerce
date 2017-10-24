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
        
        private readonly ISiteService _siteService;
        private readonly IAuthorizer _authorizer;
        private readonly ITerritoriesRepositoryService _territoryRepositoryService;
        private readonly ITransactionManager _transactionManager;

        public TerritoriesAdminController(
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizer authorizer,
            ITerritoriesRepositoryService territoryRepositoryService,
            ITransactionManager transactionManager,
            IContentDefinitionManager contentDefinitionManager) {
            
            _siteService = siteService;
            _authorizer = authorizer;
            _territoryRepositoryService = territoryRepositoryService;
            _transactionManager = transactionManager;

            _shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;

            //_allowedTerritoryTypes = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedTerritoryTypes);


            default401TerritoryMessage = T("Not authorized to manage territories.").Text;
            creation401TerritoryMessage = T("Couldn't create territory");
            edit401TerritoryMessage = T("Couldn't edit territory");
            delete401TerritoryMessage = T("Couldn't delete territory");
        }

        public Localizer T;
        dynamic _shapeFactory;
        
        string default401TerritoryMessage; //displayed for HttpUnauthorizedResults
        LocalizedString creation401TerritoryMessage;
        LocalizedString edit401TerritoryMessage;
        LocalizedString delete401TerritoryMessage;
        
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

        //[HttpGet]
        //public ActionResult HierarchyTerritoriesIndex(int id) {
        //    // list the first level of territories for the selected hierarchy
        //    if (AllowedHierarchyTypes == null) {
        //        return new HttpUnauthorizedResult(default401HierarchyMessage);
        //    }
        //    if (AllowedTerritoryTypes == null) {
        //        return new HttpUnauthorizedResult(default401TerritoryMessage);
        //    }

        //    var hierarchyItem = _contentManager.Get(id, VersionOptions.Latest);
        //    if (hierarchyItem == null) {
        //        return HttpNotFound();
        //    }
        //    var hierarchyPart = hierarchyItem.As<TerritoryHierarchyPart>();
        //    if (hierarchyPart == null) {
        //        return HttpNotFound();
        //    }

        //    if (!AllowedHierarchyTypes.Any(ty => ty.Name == hierarchyItem.ContentType)) {
        //        var typeName = _contentDefinitionManager.GetTypeDefinition(hierarchyItem.ContentType).DisplayName;
        //        return new HttpUnauthorizedResult(T("Not authorized to manage hierarchies of type \"{0}\"", typeName).Text);
        //    }
        //    if (!AllowedTerritoryTypes.Any(ty => ty.Name == hierarchyPart.TerritoryType)) {
        //        var typeName = _contentDefinitionManager.GetTypeDefinition(hierarchyPart.TerritoryType).DisplayName;
        //        return new HttpUnauthorizedResult(T("Not authorized to manage hierarchies of type \"{0}\"", typeName).Text);
        //    }

        //    return null;
        //}

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

        //private Lazy<IEnumerable<ContentTypeDefinition>> _allowedTerritoryTypes;
        //private IEnumerable<ContentTypeDefinition> AllowedTerritoryTypes {
        //    get { return _allowedTerritoryTypes.Value; }
        //}

        ///// <summary>
        ///// This method gets all the territory types the current user is allowed to manage.
        ///// </summary>
        ///// <returns>Returns the types the user is allwoed to manage. Returns null if the user lacks the correct 
        ///// permissions to be invoking these actions.</returns>
        //private IEnumerable<ContentTypeDefinition> GetAllowedTerritoryTypes() {
        //    var allowedTypes = _territoriesService.GetTerritoryTypes();
        //    if (!allowedTypes.Any() && //no dynamic permissions
        //        !_authorizer.Authorize(TerritoriesPermissions.ManageTerritories)) {

        //        return null;
        //    }

        //    return allowedTypes;
        //}


    }
}
