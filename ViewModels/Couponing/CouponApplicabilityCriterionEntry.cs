using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    public class CouponApplicabilityCriterionEntry {
        public int CriterionRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
    }
}
