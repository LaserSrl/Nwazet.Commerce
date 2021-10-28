using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.ProductCombinations")]
    [Admin]
    public class CombinationsAdminController : Controller {
        private readonly IContentManager _contentManager;

        public CombinationsAdminController(
            IContentManager contentManager) {

            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public JsonResult GenerateCombinations(int contentId, IEnumerable<AttributesToCombine> selectedAttributes) {
            // method called through ajax
            // The content we are editing may not have been published yet.
            var containerContent = _contentManager.Get(contentId, VersionOptions.Latest)
                ?.As<CombinationContainerPart>();
            if (containerContent == null) {
                // exception
                return ErrorJson(T("Invalid container."));
            }
            // group all selected values for each attribute
            var attributeValues = selectedAttributes
                .GroupBy(atc => atc.AttributeId)
                .Select(g => new AttributeValues {
                    Id = g.Key,
                    Values = g.Select(atc => atc.AttributeValue).Distinct()
                });

            return ErrorJson(T("Unknown error while generating combinations."));
        }

        class AttributeValues {
            public AttributeValues() {
                Values = new List<string>();
            }
            public int Id { get; set; }
            public IEnumerable<string> Values { get; set; }
        }
        private JsonResult ErrorJson(LocalizedString message) {
            return Json(new { ko = "ko", message = message.Text });
        }
    }
}
