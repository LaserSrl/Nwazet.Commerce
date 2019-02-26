using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class ApplicabilityCriterionRecord {

        public virtual int Id { get; set; } // Primary Key
        public virtual string Description { get; set; }
        public virtual string Type { get; set; }
        public virtual string State { get; set; }

        public virtual FlexibleShippingMethodRecord FlexibleShippingMethodRecord { get; set; }
    }
}
