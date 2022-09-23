using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartRecord : ContentPartRecord {

        public virtual IList<CombinationPartRecord> CombinationPartRecords { get; set; }
    }
}
