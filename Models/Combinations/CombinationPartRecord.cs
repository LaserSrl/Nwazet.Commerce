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
        // This property will be changed when we rework attribute values to be their own records
        // when it will be probably replaced by a list of records.
        // Right now it's the json serialization of Selected AttributesToCombine
        public virtual string ProductAttributeValues { get; set; }
    }
}
