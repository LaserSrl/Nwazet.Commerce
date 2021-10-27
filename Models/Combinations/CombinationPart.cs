using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPart : ContentPart<CombinationPartRecord> {
        // The parent product this combination is originated from
        // The attributes and attribute values that "make" this Combination
    }
}
