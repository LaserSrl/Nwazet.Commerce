using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Attributes")]
    public class ProductAttributePartRecord : ContentPartRecord {
        public ProductAttributePartRecord() {
            AttributeValueRecords = new List<ProductAttributeValueRecord>();
        }

        [StringLengthMax]
        public virtual string AttributeValues { get; set; }
        public virtual int SortOrder { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string TechnicalName { get; set; }
        public virtual string CssName { get; set; }
        public virtual string Meaning { get; set; }
        [CascadeAllDeleteOrphan, Aggregate]
        public virtual IList<ProductAttributeValueRecord> AttributeValueRecords { get; set; }
    }
}
