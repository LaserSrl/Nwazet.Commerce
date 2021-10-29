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
            // group all selected values for each attribute:
            // Each object in this list contains, for a specific Attribute, the values
            // that had been selected.
            var attributeValues = selectedAttributes
                .GroupBy(atc => atc.AttributeId)
                .Select(g => new AttributeValues {
                    Id = g.Key,
                    Values = g.Select(atc => atc.AttributeValue).Distinct()
                });
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
            // Create the new contents
            // Invoke something to start synchronizing values from the Container
            // to the Combinations

            return ErrorJson(T("Unknown error while generating combinations."));
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

        //private IEnumerable<IEnumerable<AttributesToCombine>> GetAllCombinations() {

        //}

        private JsonResult ErrorJson(LocalizedString message) {
            return Json(new { ko = "ko", message = message.Text });
        }
    }
}
