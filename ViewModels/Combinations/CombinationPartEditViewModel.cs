using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Combinations {
    public class CombinationPartEditViewModel {

        public CombinationPart Part { get; set; }
        public CombinationContainerPart CombinationContainer { get; set; }
        // The attributes we can use to generate combination
        public IEnumerable<ProductAttributePart> AllAttributeParts { get; set; }
    }
}
