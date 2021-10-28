using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class AttributesToCombine {
        public int AttributeId { get; set; }
        public string AttributeValue { get; set; }
    }
}
