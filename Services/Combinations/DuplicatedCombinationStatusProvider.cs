using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class DuplicatedCombinationStatusProvider :
        BaseCombinationStatusProvider {

        private readonly IContentManager _contentManager;

        public DuplicatedCombinationStatusProvider(
            IContentManager contentManager) {
            T = NullLocalizer.Instance;

            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

        public override List<CombinationStatusMessage> CombinationStatus(
           CombinationContainerPart container, CombinationPart combination) {

            var status = new List<CombinationStatusMessage>();

            // check whether the combination has attributes
            if(combination.ProductAttributeValues == null) {
                status.Add(new CombinationStatusMessage {
                    Severity = Severity.Error,
                    Message = T("(No attribute has been selected)").Text
                });
                return status;
            }

            // Get existing combinations
            var combinationContents = container.CombinationParts;

            // check if there is another combination in the container given the combination
            // if it is present for the combination return duplicate

            // removed attribute of the input combination
            var otherCombination = combinationContents.Where(c=> c.Id != combination.Id);

            // check for each combination
            foreach (var comb in otherCombination) {
                bool checkAttribute = true;
                // check the attributes of the input combination
                if(comb.ProductAttributeValues == null) {
                    checkAttribute = false;
                } else {
                    foreach (var attr in combination.ProductAttributeValues) {
                        // if one of the attributes is different
                        if (!comb.ProductAttributeValues
                            .Any(a => a.AttributeId == attr.AttributeId && a.AttributeValue == attr.AttributeValue)) {
                            // the message must not be shown
                            checkAttribute = false;
                        }
                    }
                }
                // if it has not entered the condition both attributes are equal
                if (checkAttribute) {
                    // exit the loop and return the message
                    status.Add(new CombinationStatusMessage {
                        Severity = Severity.Error,
                        Message = T("(Duplicated)").Text
                    });
                    break;
                }
            }

            return status;
        }
    }
}
