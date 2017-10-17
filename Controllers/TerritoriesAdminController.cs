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

        public TerritoriesAdminController(
            IContentManager contentManager,
            ITerritoriesService territoriesService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizer authorizer,
            ITerritoriesRepositoryService territoryRepositoryService,
            ITransactionManager transactionManager) {

            _contentManager = contentManager;
            _territoriesService = territoriesService;
            _siteService = siteService;
            _authorizer = authorizer;
            _territoryRepositoryService = territoryRepositoryService;
            _transactionManager = transactionManager;

            _shapeFactory = shapeFactory;
        }

        dynamic _shapeFactory;

        #region Manage the contents for territories and hierarches
        /// <summary>
        /// This is the entry Action to the section to manage hierarchies of territories and the territory ContentItems. 
        /// From here, users will not directly go and handle the records with the unique territory definitions.
        /// </summary>
        [HttpGet]
        public ActionResult HierarchiesIndex(PagerParameters pagerParameters) {
            var allowedTypes = _territoriesService.GetHierarchyTypes();
            if (!allowedTypes.Any() && //no dynamic permissions
                !_authorizer.Authorize(TerritoriesPermissions.ManageTerritoryHierarchies)) {

                return new HttpUnauthorizedResult();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            HierarchyAdminIndexViewModel model;
            if (allowedTypes.Any()) {

                var hierarchies = _territoriesService
                    .GetHierarchiesQuery()
                    .ForType(allowedTypes.Select(ctd => ctd.Name).ToArray())
                    .Slice(pager.GetStartIndex(), pager.PageSize);

                var pagerShape = _shapeFactory
                    .Pager(pager)
                    .TotalItemCount(_territoriesService.GetHierarchiesQuery().Count());

                var entries = hierarchies
                        .Select(CreateEntry)
                        .ToList();

                model = new HierarchyAdminIndexViewModel { Hierarchies = entries, Pager = pagerShape };
            } else {
                //No ContentType has been defined that contains the TerritoryHierarchyPart
                //TODO: handle this condition in its own way
                var pagerShape = _shapeFactory
                    .Pager(pager)
                    .TotalItemCount(0);

                model = new HierarchyAdminIndexViewModel { Hierarchies = new List<HierarchyIndexEntry>(), Pager = pagerShape };
            }

            return View(model);
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
            //TODO: Add bulk actions: None, Delete Selected, Delete All, Export

            return View((object)viewModel);
        }

        [HttpGet]
        public ActionResult AddTerritory() {
            return View();
        }

        [HttpPost, ActionName("AddTerritory")]
        public ActionResult AddTerritoryPost() {
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

        public HierarchyIndexEntry CreateEntry(TerritoryHierarchyPart part) {
            return new HierarchyIndexEntry {
                Id = part.ContentItem.Id,
                DisplayTest = _contentManager.GetItemMetadata(part.ContentItem).DisplayText,
                ContentItem = part.ContentItem,
                IsDraft = !part.ContentItem.IsPublished(),
                TerritoriesCount = part.Territories.Count()
            };
        }
    }
}
