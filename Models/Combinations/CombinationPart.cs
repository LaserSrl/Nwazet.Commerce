using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
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
        private readonly LazyField<CombinationContainerPart> _combinationContainerPart =
            new LazyField<CombinationContainerPart>();
        public LazyField<CombinationContainerPart> CombinationContainerPartField {
            get { return _combinationContainerPart; }
        }
        public CombinationContainerPart CombinationContainerPart {
            get { return _combinationContainerPart.Value; }
        }
        // The attributes and attribute values that "make" this Combination
        private readonly LazyField<ProductAttributePart> _productAttributePart =
            new LazyField<ProductAttributePart>();
        public LazyField<ProductAttributePart> ProductAttributePartField {
            get { return _productAttributePart; }
        }
        public ProductAttributePart ProductAttributePart {
            get { return _productAttributePart.Value; }
        }
        public string ProductAttributeValue {
            get { return Retrieve(r => r.ProductAttributeValue); }
            set { Store(r => r.ProductAttributeValue, value); }
        }
    }
}
