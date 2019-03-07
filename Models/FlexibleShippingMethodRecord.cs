using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodRecord : ContentPartRecord {
        public virtual string Name { get; set; }
        public virtual string ShippingCompany { get; set; }
        public virtual string IncludedShippingAreas { get; set; }
        public virtual string ExcludedShippingAreas { get; set; }
        public virtual decimal DefaultPrice { get; set; }

        // Since this is a ContentPartRecord it does not get deleted from the db when the
        // ContentItem it belongs to is deleted. Thus, the CascadeAllDeleteOrphan attribute
        // will not cause child criteria to be deleted there. They would have to be deleted
        // explicitly. In keeping with the approaches used in Orchard.Projections, the child
        // records will not be deleted when the ContentItem is removed.
        // It is required anyway to allow saving the children records.
        [CascadeAllDeleteOrphan, Aggregate]
        [XmlArray("ApplicabilityCriteria")]
        public virtual IList<ApplicabilityCriterionRecord> ApplicabilityCriteria { get; set; }
    }
}
