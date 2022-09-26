using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.ViewModels.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class AttributesToCombine {
        public int AttributeId { get; set; }
        public int AttributeValue { get; set; } // Id of the AttributeValueRecord

        public override bool Equals(object obj) {
            if (!(obj is AttributesToCombine)) {
                return base.Equals(obj);
            }
            var other = (AttributesToCombine)obj;
            return other.AttributeId == this.AttributeId
                && other.AttributeValue == this.AttributeValue;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
