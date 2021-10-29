using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPartRecord : ContentPartRecord {

        /// <summary>
        /// Record of parent.
        /// </summary>
        [Aggregate]
        public virtual CombinationContainerPartRecord CombinationContainerPartRecord { get; set; }

        // Describe "how" this combination is made.
    }
}
