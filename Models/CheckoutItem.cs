using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Models {
    [Serializable]
    public sealed class CheckoutItem {

        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal OriginalPrice { get; set; }

        public decimal LinePriceAdjustment { get; set; }
        public string Title { get; set; }
        public IDictionary<int, ProductAttributeValueExtended> Attributes { get; set; }

        // Old order items will have a null promotionId, this ensures it will be defaulted to a 0.
        int _promotionId;
        public int? PromotionId {
            get {
                return this._promotionId;
            }
            set {
                this._promotionId = value ?? 0;
            }
        }
        
        public override string ToString() {
            return Quantity + " x " + Title + " " + Price.ToString("C");
        }

        private string _uniqueKey; //lazy
        /// <summary>
        /// Returns a string representation for the CheckoutItem that will uniquely
        /// represent it within an Order. This representation will not be unique across
        /// all possible Orders, but it whould be such that within a given order, no two
        /// ChecktouItems have the same.
        /// </summary>
        /// <returns></returns>
        public string AsUniqueKey() {
            if (string.IsNullOrWhiteSpace(_uniqueKey)) {
                // We should be accounting for product attributes:
                //  - having different attributes means we may have multiple lines in the order for a product with
                //    the same Id.
                //  - It means that the way this data is stored has to be adapted to accomodate for it.
                //  - While they would have the same VAT Rate (at least for now), those multiple lines could in
                //    principle have different prices
                //  - That means that the product's Id is not enough of a key
                if (Attributes == null || !Attributes.Any()) {
                    // this is like this for retrocompatibility with the time attributes were
                    // not considered correctly.
                    _uniqueKey = ProductId.ToString();
                } else {
                    // there are attributes
                    var key = new KeyFormat {
                        ProductId = this.ProductId,
                        Attributes = this.Attributes as Dictionary<int, ProductAttributeValueExtended>
                    };

                    _uniqueKey = JsonConvert.SerializeObject(key, Formatting.None);
                }
            }
            return _uniqueKey;
        }

        struct KeyFormat {
            public int ProductId { get; set; }
            public Dictionary<int, ProductAttributeValueExtended> Attributes { get; set; }
        }
    }
}