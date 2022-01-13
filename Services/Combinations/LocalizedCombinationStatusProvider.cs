using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class LocalizedCombinationStatusProvider :
        BaseCombinationStatusProvider {

        private readonly IContentManager _contentManager;

        public LocalizedCombinationStatusProvider(
            IContentManager contentManager) {
            T = NullLocalizer.Instance;

            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

        public override List<CombinationStatusMessage> CombinationStatus(
           CombinationContainerPart container, CombinationPart combination) {

            var status = new List<CombinationStatusMessage>();

            var localizationCombination = combination.ContentItem.As<LocalizationPart>();
            var localizationContainer = container.ContentItem.As<LocalizationPart>();

            // saved culture of combination and container 
            // to use it in the message 
            var cultureCombination = T("Not selected").Text;
            if(localizationCombination != null && 
                localizationCombination.Culture != null &&
                !string.IsNullOrEmpty(localizationCombination.Culture.Culture)) {
                cultureCombination = localizationCombination.Culture.Culture;
            }
            var cultureContainer = T("Not selected").Text;
            if (localizationContainer != null &&
               localizationContainer.Culture != null &&
               !string.IsNullOrEmpty(localizationContainer.Culture.Culture)) {
                cultureContainer = localizationContainer.Culture.Culture;
            }

            if (localizationCombination != null) {
                // check if the combination has the culture
                if (localizationCombination.Culture == null) {
                    status.Add(new CombinationStatusMessage {
                        Severity = Severity.Error,
                        Message = T("(Missing culture)").Text
                    });
                }
                // check if the combination has the same culture as the container
                if (localizationContainer != null) {
                    if(localizationCombination.Culture != localizationContainer.Culture) {
                        status.Add(new CombinationStatusMessage {
                            Severity = Severity.Warning,
                            Message = T("(Different culture. Combination: {0} - Container: {1})",
                                cultureCombination,
                                cultureContainer).Text
                        });
                    }
                }
            } else {
                // if the combination does not have the culture and the container does, whatever, message
                if (localizationContainer != null) {
                    status.Add(new CombinationStatusMessage {
                        Severity = Severity.Warning,
                        Message = T("(The combination has no localization)").Text
                    });
                }
            }
            return status;
        }
    }
}
