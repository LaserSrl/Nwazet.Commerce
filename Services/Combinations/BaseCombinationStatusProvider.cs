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
        public BaseCombinationStatusProvider() {
            T = NullLocalizer.Instance;
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
            return status;
        }
    }
}
