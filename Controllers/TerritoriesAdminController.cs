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

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    [ValidateInput(false)]
    public class TerritoriesAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly ITerritoriesService _territoriesService;
        private readonly ISiteService _siteService;

        public TerritoriesAdminController(
            IOrchardServices orchardServices,
            ITerritoriesService territoriesService,
            ISiteService siteService) {

            _orchardServices = orchardServices;
            _territoriesService = territoriesService;
            _siteService = siteService;
        }

        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var taxonomies = _territoriesService.GetHierarchiesQuery().Slice(pager.GetStartIndex(), pager.PageSize);

            var pagerShape = Shape.Pager(pager).TotalItemCount(_taxonomyService.GetTaxonomiesQuery().Count());

            var entries = taxonomies
                    .Select(CreateTaxonomyEntry)
                    .ToList();

            var model = new TaxonomyAdminIndexViewModel { Taxonomies = entries, Pager = pagerShape };

            return View(model);
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
