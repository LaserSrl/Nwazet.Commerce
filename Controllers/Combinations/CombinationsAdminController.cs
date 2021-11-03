using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Combinations;
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
        private readonly IProductCombinationService _productCombinationService;

        public CombinationsAdminController(
            IContentManager contentManager,
            IProductCombinationService productCombinationService) {

            _contentManager = contentManager;
            _productCombinationService = productCombinationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public JsonResult GenerateCombinations(int contentId, IEnumerable<AttributesToCombine> selectedAttributes) {
            // method called through ajax
            // TODO check user permissions
            // The content we are editing may not have been published yet.
            var containerContent = _contentManager.Get(contentId, VersionOptions.Latest)
                ?.As<CombinationContainerPart>();
            if (containerContent == null) {
                // exception
                return ErrorJson(T("Invalid container."));
            }
            // group all selected values for each attribute:
            // Each object in this list contains, for a specific Attribute, the values
            // that had been selected.
            var attributeValues = selectedAttributes
                .GroupBy(atc => atc.AttributeId)
                .Select(g => new AttributeValues {
                    Id = g.Key,
                    Values = g.Select(atc => atc.AttributeValue).Distinct()
                });
            // TODO: validation of attributes and corresponding values
            // recursively build the combinations:
            // We are going to create a collection of combinations. Each combination
            // has a list of <AttributeId, AttributeValue> pairs such that:
            // - there are no duplicate combinations
            // - There is only one value for each AttributeId
            var allElements = attributeValues.Select(av => av.GetElements());
            IEnumerable<IEnumerable<AttributesToCombine>> allCombinations = 
                new List<List<AttributesToCombine>>() { new List<AttributesToCombine>() };
            foreach (var items in allElements) {
                allCombinations = allCombinations
                    // for each current combination
                    //   for each element of items
                    //     return a new list obtained by adding the element to the combination
                    .SelectMany(i => items, (combo, att) => combo.Append(att));
                // This is equivalent to the following cross join:
                // allCombinations = from c in allCombinations
                //                   from i in items
                //                   select c.Append(i);
            }
            // Select the combinations for which there isn't yet a product in the
            // container
            var currentCombinationRecords = containerContent.Record.CombinationPartRecords;
            var currentCombinations = currentCombinationRecords
                .Select(cpr => CombinationPart.DeserializeCombinations(cpr));
            var newCombinations = allCombinations
                .Where(combo => {
                    foreach (var current in currentCombinations) {
                        // combo.Except(current).Any()
                        if (combo.Count() == current.Count()
                            && !combo.Any(com => 
                                !current.Any(cur=> 
                                    cur.AttributeId == com.AttributeId && cur.AttributeValue.Equals(com.AttributeValue)))) {
                            // combo and current represent the same combination
                            return false;
                        }
                    }
                    return true;
                }).ToList();
            // Create the new contents
            var created = _productCombinationService
                .CreateCombinations(containerContent, newCombinations);

            return SuccessJson(T("{0} new combinations created.", created.Count()));
        }

        class AttributeValues {
            public AttributeValues() {
                Values = new List<string>();
            }
            public int Id { get; set; }
            public IEnumerable<string> Values { get; set; }

            public IEnumerable<AttributesToCombine> GetElements() {
                return Values.Select(v => new AttributesToCombine { AttributeId = Id, AttributeValue = v });
            }
        }
        
        private JsonResult ErrorJson(LocalizedString message) {
            return Json(new { ko = "ko", message = message.Text });
        }

        private JsonResult SuccessJson(LocalizedString message) {
            return Json(new { ok = "ok", message = message.Text });
        }
    }
}
