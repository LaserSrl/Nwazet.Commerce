using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class TerritoryVatConfigurationPartRecord : ContentPartRecord {

        public virtual decimal Rate { get; set; }
    }
}
