using Nwazet.Commerce.Services;
using System.Collections.Generic;

namespace Nwazet.Commerce.Models {
    public class ShippingOption {
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ShippingCompany { get; set; }
        public IEnumerable<string> IncludedShippingAreas { get; set; }
        public IEnumerable<string> ExcludedShippingAreas { get; set; }
        public string FormValue { get; set; }
        /// <summary>
        /// Adding a reference to the shipping method for which this option was generated
        /// allows us to backtrack and eventually provide more information. This makes us
        /// more flexible, because IShippingMethod implementations may be used to provide
        /// additional stuff at all stages.
        /// </summary>
        public IShippingMethod ShippingMethod { get; set; }

        public override string ToString() {
            return Description + ": $" + Price.ToString("F2");
        }

        public class ShippingOptionComparer : IEqualityComparer<ShippingOption> {
            public bool Equals(ShippingOption x, ShippingOption y) {
                if (x == null) {
                    return y == null;
                }
                return x.Price.Equals(y.Price) &&
                    x.Description == y.Description &&
                    x.ShippingCompany == y.ShippingCompany;
            }

            public int GetHashCode(ShippingOption obj) {
                if (obj == null) return 0;
                return new {obj.Description, obj.Price, obj.ShippingCompany}.GetHashCode();
            }
        }
    }
}
