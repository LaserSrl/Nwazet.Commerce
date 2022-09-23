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
    public class BaseCombinationStatusProvider : ICombinationStatusProvider {
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IContentManager _contentManager;

        private Dictionary<int, string> _attributeNames;
        private Dictionary<int, string> _attributeValueText;

        public BaseCombinationStatusProvider(IProductAttributeAdminServices productAttributeAdminServices,
            IContentManager contentManager) {
            _productAttributeAdminServices = productAttributeAdminServices;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;

            _attributeNames = new Dictionary<int, string>();
            _attributeValueText = new Dictionary<int, string>();
        }

        public Localizer T { get; set; }

        public virtual List<CombinationStatusMessage> CombinationStatus(
            CombinationContainerPart container, CombinationPart combination) {
            // basic checks on the status of the combination
            var status = new List<CombinationStatusMessage>();

            // check if it is published
            if (combination.HasPublished()) {
                status.Add(new CombinationStatusMessage {
                    Severity = Severity.Information,
                    Message = T("(Published)").Text
                });
            } else {
                status.Add(new CombinationStatusMessage {
                    Severity = Severity.Information,
                    Message = T("(Not Published)").Text
                });
            }

            // check if it is a draft
            if (combination.HasDraft()) {
                status.Add(new CombinationStatusMessage {
                    Severity = Severity.Information,
                    Message = T("(Draft)").Text
                });
            } else {
                status.Add(new CombinationStatusMessage {
                    Severity = Severity.Information,
                    Message = T("(No Draft)").Text
                });
            }

            // Check if the combination has valid attributes.
            var comboValues = combination.ProductAttributeValues;
            var attributeIdsToFetch = comboValues == null ? new List<int>() : comboValues
                .Select(cv => cv.AttributeId)
                .Except(_attributeNames.Keys);
            var attributes = _productAttributeAdminServices
                .GetProductAttributeParts(attributeIdsToFetch.ToArray());

            foreach (var newAttribute in attributes) {
                var attDisplayText = _contentManager.GetItemMetadata(newAttribute).DisplayText;
                // TODO: fallbacks for the displaytext
                if (!_attributeNames.ContainsKey(newAttribute.Id)) {
                    _attributeNames.Add(newAttribute.Id, attDisplayText);
                }
                // Memorize the text for the attribute values
                foreach (var attVal in newAttribute.AttributeValues) {
                    if (!_attributeValueText.ContainsKey(attVal.Id)) {
                        _attributeValueText.Add(attVal.Id, attVal.Text);
                    }
                }
            }

            var invalidAttributes = comboValues.Any(cv => !_attributeNames.ContainsKey(cv.AttributeId));
            var invalidValues = comboValues.Any(cv => !_attributeValueText.ContainsKey(cv.AttributeValue));
            if (invalidAttributes || invalidValues) {
                status.Add(new CombinationStatusMessage {
                    Severity = Severity.Error,
                    Message = T("(Invalid Attributes)").Text
                });
            }

            return status;
        }
    }
}
