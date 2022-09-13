using Newtonsoft.Json;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

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
        public IEnumerable<AttributesToCombine> ProductAttributeValues {
            get {
                var serialized = Retrieve(r => r.ProductAttributeValues) ?? string.Empty;
                return JsonConvert.DeserializeObject<List<AttributesToCombine>>(serialized);
            }
            set {
                var serialized = JsonConvert.SerializeObject(value);
                Store(r => r.ProductAttributeValues, serialized);
            }
        }

        //public IEnumerable<ProductAttributePart> ProductAttributeParts {
        //    get {
        //        return Retrieve(r => r.ProductAttributeParts);
        //    }
        //    set {
        //        Store(r => r.ProductAttributeParts, value);
        //    }
        //}

        public static IEnumerable<AttributesToCombine> DeserializeCombinations(
            CombinationPartRecord record) {
            var serialized = record.ProductAttributeValues ?? string.Empty;
            return JsonConvert.DeserializeObject<List<AttributesToCombine>>(serialized);
        }

        public static IEnumerable<AttributesToCombine> DeserializeCombinations(
            CombinationPart part) {
            return DeserializeCombinations(part.Record);
        }
    }
}
