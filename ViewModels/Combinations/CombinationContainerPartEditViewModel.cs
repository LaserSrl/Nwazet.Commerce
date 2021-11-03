using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartEditViewModel {

        public CombinationContainerPartEditViewModel() {

            AllAttributeParts = new List<ProductAttributePart>();
        }

        public CombinationContainerPart Part { get; set; }

        // This comes from the Part's Settings
        public string CombinationTypeName { get; set; }

        // The attributes we can use to generate combination
        public IEnumerable<ProductAttributePart> AllAttributeParts { get; set; }

        // Current existing combinations
        public IEnumerable<CombinationPart> CurrentCombinations { get; set; }
    }
}
