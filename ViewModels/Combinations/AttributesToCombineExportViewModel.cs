using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.ViewModels.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    class AttributesToCombineExportViewModel {
        /// <summary>
        /// Identifier of the ProductAttribute
        /// </summary>
        public string AttributeId { get; set; }
        /// <summary>
        /// GUIdentifier of the ProductAttributeValue
        /// </summary>
        public string ValueId { get; set; }
    }
}
