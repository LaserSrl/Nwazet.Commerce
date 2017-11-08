using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
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
using System.Web.Routing;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    [Admin]
    [ValidateInput(false)]
    public class HierarchyTerritoriesAdminController : Controller, IUpdateModel {

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITerritoriesService _territoriesService;
        private readonly IAuthorizer _authorizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly RouteCollection _routeCollection;

        public HierarchyTerritoriesAdminController(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITerritoriesService territoriesService,
            IAuthorizer authorizer,
            IWorkContextAccessor workContextAccessor,
            RouteCollection routeCollection) {

            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _territoriesService = territoriesService;
            _authorizer = authorizer;
            _workContextAccessor = workContextAccessor;
            _routeCollection = routeCollection;

            T = NullLocalizer.Instance;

            _allowedTerritoryTypes = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedTerritoryTypes);
            _allowedHierarchyTypes = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedHierarchyTypes);

        }

        public Localizer T;

        [HttpGet]
        public ActionResult Index(int id) {
            // list the first level of territories for the selected hierarchy
            if (AllowedHierarchyTypes == null) {
                return new HttpUnauthorizedResult(TerritoriesUtilities.Default401HierarchyMessage);
            }
            if (AllowedTerritoryTypes == null) {
                return new HttpUnauthorizedResult(TerritoriesUtilities.Default401TerritoryMessage);
            }

            var hierarchyItem = _contentManager.Get(id, VersionOptions.Latest);
            if (hierarchyItem == null) {
                return HttpNotFound();
            }
            var hierarchyPart = hierarchyItem.As<TerritoryHierarchyPart>();
            if (hierarchyPart == null) {
                return HttpNotFound();
            }

            if (!AllowedHierarchyTypes.Any(ty => ty.Name == hierarchyItem.ContentType)) {
                var typeName = _contentDefinitionManager.GetTypeDefinition(hierarchyItem.ContentType).DisplayName;
                return new HttpUnauthorizedResult(TerritoriesUtilities.SpecificHierarchy401Message(typeName));
            }
            if (!AllowedTerritoryTypes.Any(ty => ty.Name == hierarchyPart.TerritoryType)) {
                var typeName = _contentDefinitionManager.GetTypeDefinition(hierarchyPart.TerritoryType).DisplayName;
                return new HttpUnauthorizedResult(TerritoriesUtilities.SpecificTerritory401Message(typeName));
            }

            var firstLevelOfHierarchy = _territoriesService
                .GetTerritoriesQuery(hierarchyPart, null, VersionOptions.Latest)
                .List().ToList();
                       

            var model = new TerritoryHierarchyTerritoriesViewModel {
                HierarchyPart = hierarchyPart,
                HierarchyItem = hierarchyItem,
                Nodes = firstLevelOfHierarchy.Select(MakeANode).ToList()
            };

            return View(model);
        }

        private Lazy<IEnumerable<ContentTypeDefinition>> _allowedTerritoryTypes;
        private IEnumerable<ContentTypeDefinition> AllowedTerritoryTypes {
            get { return _allowedTerritoryTypes.Value; }
        }

        /// <summary>
        /// This method gets all the territory types the current user is allowed to manage.
        /// </summary>
        /// <returns>Returns the types the user is allwoed to manage. Returns null if the user lacks the correct 
        /// permissions to be invoking these actions.</returns>
        private IEnumerable<ContentTypeDefinition> GetAllowedTerritoryTypes() {
            var allowedTypes = _territoriesService.GetTerritoryTypes();
            if (!allowedTypes.Any() && //no dynamic permissions
                !_authorizer.Authorize(TerritoriesPermissions.ManageTerritories)) {

                return null;
            }

            return allowedTypes;
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

        private TerritoryHierarchyTreeNode MakeANode(TerritoryPart territoryPart) {
            var metadata = _contentManager.GetItemMetadata(territoryPart.ContentItem);
            var requestContext = _workContextAccessor.GetContext().HttpContext.Request.RequestContext;
            return new TerritoryHierarchyTreeNode {
                Id = territoryPart.ContentItem.Id,
                EditUrl = _routeCollection.GetVirtualPath(requestContext, metadata.EditorRouteValues).VirtualPath,
                DisplayText = metadata.DisplayText
            };
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
