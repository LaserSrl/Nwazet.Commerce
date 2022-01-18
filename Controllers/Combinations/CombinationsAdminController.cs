using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using CorePermissions = Orchard.Core.Contents.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.Mvc.Html;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.ProductCombinations")]
    [Admin]
    public class CombinationsAdminController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IProductCombinationService _productCombinationService;
        private readonly IAuthorizer _authorizer;
        private readonly Lazy<IEnumerable<ICombinationStatusProvider>> _combinationStatusProvider;
        protected UrlHelper _url;

        public CombinationsAdminController(
            IContentManager contentManager,
            IProductCombinationService productCombinationService,
            IAuthorizer authorizer,
            Lazy<IEnumerable<ICombinationStatusProvider>> combinationStatusProvider,
            UrlHelper url) {

            _contentManager = contentManager;
            _productCombinationService = productCombinationService;
            _authorizer = authorizer;
            _combinationStatusProvider = combinationStatusProvider;
            _url = url;
            
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private IEnumerable<ICombinationStatusProvider> CombinationStatusProviders {
            // provider used for status messages of a combination
            // is checked for each combination: 
            // the status if published or draft
            // the culture
            // if it is a duplicate
            // returning for each provider a message and a severity
            get { return _combinationStatusProvider.Value; }
        }

        [HttpPost]
        [Authorize]
        public JsonResult GenerateCombinations(int contentId, IEnumerable<AttributesToCombine> selectedAttributes) {
            // method called through ajax
            // The content we are editing may not have been published yet.
            var containerContent = _contentManager.Get(contentId, VersionOptions.Latest)
                ?.As<CombinationContainerPart>();
            if (containerContent == null) {
                // exception
                return ErrorJson(T("Invalid container."));
            }
            // get a Combination to test whether the user is allowed to create new ones
            var dummyCombination = _productCombinationService.GetDummyCombination(containerContent);
            if (!_authorizer.Authorize(CorePermissions.EditContent, containerContent)
                || !_authorizer.Authorize(CorePermissions.CreateContent, dummyCombination)) {
                // unauthorized
                return ErrorJson(T("Unauthorized."));
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

            // Recursively build the combinations:
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
            var currentCombinations = containerContent
                .CombinationParts
                .Select(cp => CombinationPart.DeserializeCombinations(cp));
            var newCombinations = allCombinations
                .Where(combo => {
                    foreach (var current in currentCombinations) {
                        // combo.Except(current).Any()
                        if(current != null) {
                            if (combo.Count() == current.Count()
                                && !combo.Any(com => 
                                    !current.Any(cur=> 
                                        cur.AttributeId == com.AttributeId && cur.AttributeValue.Equals(com.AttributeValue)))) {
                                // combo and current represent the same combination
                                return false;
                            }
                        }
                    }
                    return true;
                }).ToList();
            // Create the new contents
            var created = _productCombinationService
                .CreateCombinations(containerContent, newCombinations);
            // return the combinations we created to update the UI directly without having
            // to reload the page.
            return SuccessJson(T("{0} new combinations created.", created.Count()), containerContent, created);
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

        private JsonResult SuccessJson(
            LocalizedString message, 
            CombinationContainerPart container, 
            IEnumerable<CombinationPart> combinationsCreated) {
            // return the combinations we created to update the UI directly without having
            // to reload the page.
            var combinations = new List<JsonCombination>();
            foreach (var combination in combinationsCreated) {
                var comboStatus = new List<CombinationStatusMessage>();
                // status messages for each combination
                foreach (var provider in CombinationStatusProviders) {
                    comboStatus.AddRange(provider.CombinationStatus(container, combination));
                }
                combinations.Add(new JsonCombination {
                    Title = _productCombinationService.CombinationDisplayText(combination),
                    EditUrl= _url.ItemEditUrl((IContent)combination.ContentItem, new { returnUrl = Url.ItemEditUrl(combination.CombinationContainerPart.ContentItem) }),
                    DeleteUrl= _url.ItemRemoveUrl(combination.ContentItem, new { returnUrl = Url.ItemEditUrl(combination.CombinationContainerPart.ContentItem) }),
                    // created a viewmodel for ease of reading in the javascript script
                    // this way read only the severity string and not the bolean
                    CombinationStatus = comboStatus
                        .OrderBy(s => s.Severity)
                        .Select(c=> new CombinationStatusMessageVM { Message = c.Message, Severity = c.Severity.ToString().ToLower() }).ToList()
                });
            }

            return Json(new {
                ok = "ok",
                message = message.Text,
                newCombinations = combinations
            });
        }

        private class CombinationStatusMessageVM {
            public string Message { get; set; }
            public string Severity { get; set; }
        }
        
        class JsonCombination {
            public string Title { get; set; }
            public string EditUrl { get; set; }
            public string DeleteUrl { get; set; }
            public List<CombinationStatusMessageVM> CombinationStatus { get; set; }
        }
    }
}
