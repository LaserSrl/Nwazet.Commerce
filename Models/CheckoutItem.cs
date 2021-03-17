using Newtonsoft.Json;
using Nwazet.Commerce.Extensions;
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
                _uniqueKey = this.GenerateUniqueKey();
            }
            return _uniqueKey;
        }
        
    }
}