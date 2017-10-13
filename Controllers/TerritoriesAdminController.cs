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

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    [ValidateInput(false)]
    public class TerritoriesAdminController : Controller, IUpdateModel {

        private readonly IContentManager _contentManager;
        private readonly ITerritoriesService _territoriesService;
        private readonly ISiteService _siteService;
        private readonly IAuthorizer _authorizer;

        public TerritoriesAdminController(
            IContentManager contentManager,
            ITerritoriesService territoriesService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizer authorizer) {

            _contentManager = contentManager;
            _territoriesService = territoriesService;
            _siteService = siteService;
            _authorizer = authorizer;

            _shapeFactory = shapeFactory;
        }

        dynamic _shapeFactory;

        #region Manage the contents for territories and hierarches
        /// <summary>
        /// This is the entry Action to the section to manage hierarchies of territories and the territory ContentItems. 
        /// From here, users will not directly go and handle the records with the unique territory definitions.
        /// </summary>
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
        /// <summary>
        /// This is the entry Action to the section to manage The TerritoryIntenalRecords. These are the
        /// unique records that exist behind TerritoryPartRecords, and that are used to relate "same"
        /// TerritoryParts, also across hierarchies. For example, they can uniquely match "Cyprus" from a
        /// fiscal hierarchy with "Cyprus" from a shipping hierarchy.
        /// </summary>
        /// <param name="pagerParameters"></param>
        /// <returns></returns>
        public ActionResult TerritoriesIndex(PagerParameters pagerParameters) {
            if (!_authorizer.Authorize(TerritoriesPermissions.ManageInternalTerritories)) {
                return new HttpUnauthorizedResult();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            return View();
        }
        #endregion

        #region IUpdateModel implementation
        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
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
