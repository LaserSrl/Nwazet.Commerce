using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicabilityCriterionRecord {

        public virtual int Id { get; set; } // Primary Key
        public virtual string Category { get; set; }
        public virtual string Description { get; set; }
        public virtual string Type { get; set; }
        public virtual string State { get; set; }

        // "parent" property
        public virtual CouponRecord CouponRecord { get; set; }
    }
}
