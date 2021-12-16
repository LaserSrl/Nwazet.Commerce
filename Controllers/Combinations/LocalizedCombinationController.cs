using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class LocalizedCombinationController : Controller {
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;
        
        public LocalizedCombinationController(
            IProductAttributeAdminServices productAttributeAdminServices,
            ILocalizationService localizationService,
            IContentManager contentManager
            ) {
            _productAttributeAdminServices = productAttributeAdminServices;
            _localizationService = localizationService;
            _contentManager = contentManager;
        }

        public ActionResult GetCombination(string containerId, string culture) {
            var vm = new CombinationPartEditViewModel();
            int id;
            if(!string.IsNullOrWhiteSpace(containerId) && int.TryParse(containerId, out id)) {
                var combinationContainerPart = _contentManager.Get(id, VersionOptions.Latest)
                    ?.As<CombinationContainerPart>();

                var allAttributes = _productAttributeAdminServices
                    .GetAllProductAttributeParts();
                if (!string.IsNullOrWhiteSpace(culture)) {
                    allAttributes = allAttributes
                        .Where(a => _localizationService.GetContentCulture(a.ContentItem) == culture);
                }
                vm = new CombinationPartEditViewModel {
                    CombinationContainer = combinationContainerPart,
                    AllAttributeParts = allAttributes
                };
            }
            var templateName = "../EditorTemplates/Parts/Combinations/AttributesCombinationPart";
            return View(templateName, vm);
        }
    }
}
