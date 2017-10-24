using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
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

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    [Admin]
    [ValidateInput(false)]
    public class HierarchyTerritoriesAdminController : Controller, IUpdateModel {

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITerritoriesService _territoriesService;
        private readonly IAuthorizer _authorizer;

        public HierarchyTerritoriesAdminController(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITerritoriesService territoriesService,
            IAuthorizer authorizer) {

            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _territoriesService = territoriesService;
            _authorizer = authorizer;

            T = NullLocalizer.Instance;

            _allowedTerritoryTypes = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedTerritoryTypes);
            _allowedHierarchyTypes = new Lazy<IEnumerable<ContentTypeDefinition>>(GetAllowedHierarchyTypes);

        }

        public Localizer T;

        [HttpGet]
        public ActionResult HierarchyTerritoriesIndex(int id) {
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

            return null;
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
