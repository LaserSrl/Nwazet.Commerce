using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class HierarchyVatConfigurationPartRecord : ContentPartRecord {

        public HierarchyVatConfigurationPartRecord() {
            VatConfigurationIntersections = new List<HierarchyVatConfigurationIntersectionRecord>();
        }
        
        public virtual IList<HierarchyVatConfigurationIntersectionRecord> VatConfigurationIntersections { get; set; }
    }
}
