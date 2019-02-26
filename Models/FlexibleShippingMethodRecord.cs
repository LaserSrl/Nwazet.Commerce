using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodRecord : ContentPartRecord {
        public virtual string Name { get; set; }
        public virtual string ShippingCompany { get; set; }
        public virtual string IncludedShippingAreas { get; set; }
        public virtual string ExcludedShippingAreas { get; set; }
        public virtual decimal DefatulPrice { get; set; }

        public virtual IList<ApplicabilityCriterionRecord> ApplicabilityCriteria { get; set; }
    }
}
