using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPartRecord : ContentPartRecord {
        

        public virtual int Priority { get; set; }
        [StringLengthMax]
        public virtual string TaxProductCategory { get; set; }

        public virtual TerritoryHierarchyPartRecord Hierarchy { get; set; }

        public virtual decimal DefaultRate { get; set; }
    }
}
