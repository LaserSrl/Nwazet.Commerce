using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Settings.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartSettings {
        public string CombinationTypeName { get; set; }

        // We won't memorize this flag, but we use it to make sure the user is 
        // actually trying to change the value of CombinationTypeName.
        public bool UpdateCombinationName { get; set; }
    }
}
