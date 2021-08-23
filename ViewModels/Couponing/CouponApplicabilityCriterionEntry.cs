using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicabilityCriterionEntry {
        public int CriterionRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
        // We need to carry the state for import/export
        public string State { get; set; }

        public bool IsAvailableForConfiguration { get; set; }
        public bool IsAvailableForProcessing { get; set; }

    }
}
