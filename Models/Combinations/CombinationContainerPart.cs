using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPart : ContentPart<CombinationContainerPartRecord> {
        // The ContentType used for the combinations for this product
        // is configured as a setting for the part

        // The collection of Combinations that we have configured so far:
        // Sould this be here directly? 
        // Should we fetch this through a service "on demand"?
    }
}
