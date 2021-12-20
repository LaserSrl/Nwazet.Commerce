using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationAttribute {
        public int AttributeId { get; set; }
        public string AttributeDisplayName { get; set; }
        public List<AttributeValue> Values { get; set; }
        public int SelectedAttributeValue { get; set; }
    }

    [OrchardFeature("Nwazet.ProductCombinations")]
    public class AttributeValue {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
