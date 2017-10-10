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

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    [ValidateInput(false)]
    public class TerritoriesAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly ITerritoriesService _territoriesService;

        public TerritoriesAdminController(
            IOrchardServices orchardServices,
            ITerritoriesService territoriesService) {

            _orchardServices = orchardServices;
            _territoriesService = territoriesService;
        }

        #region IUpdateModel implementation
        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
    }
}
