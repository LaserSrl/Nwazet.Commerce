using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class HierarchyVatConfigurationIntersectionRecord {

        public virtual int Id { get; set; } // Primary Key
        public virtual HierarchyVatConfigurationPartRecord Hierarchy { get; set; }
        public virtual VatConfigurationPartRecord VatConfiguration { get; set; }
        public virtual decimal Rate { get; set; }
    }
}
