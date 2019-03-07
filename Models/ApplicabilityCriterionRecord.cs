using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class ApplicabilityCriterionRecord {

        public virtual int Id { get; set; } // Primary Key
        public virtual string Category { get; set; }
        public virtual string Description { get; set; }
        public virtual string Type { get; set; }
        public virtual string State { get; set; }

        // "parent" property
        public virtual FlexibleShippingMethodRecord FlexibleShippingMethodRecord { get; set; }
    }
}
