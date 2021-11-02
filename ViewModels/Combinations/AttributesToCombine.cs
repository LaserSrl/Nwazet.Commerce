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

        public override bool Equals(object obj) {
            if (!(obj is AttributesToCombine)) {
                return base.Equals(obj);
            }
            var other = (AttributesToCombine)obj;
            return other.AttributeId == this.AttributeId
                && ((other.AttributeValue == null && this.AttributeValue == null)
                    || other.AttributeValue.Equals(this.AttributeValue));
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
