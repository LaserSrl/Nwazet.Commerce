using Orchard.Environment.Extensions;
using System.Collections.Generic;

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
