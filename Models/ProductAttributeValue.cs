using Orchard.Environment.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Attributes")]
    public class ProductAttributeValue {
        public int Id { get; set; }
        [Required, DisplayName("Value Name")]
        public string Text { get; set; }
        [Required, DisplayName("Price Adjustment")]
        public decimal PriceAdjustment { get; set; }
        [DefaultValue(false), DisplayName("Is Line Adjustment")]
        public bool IsLineAdjustment { get; set; }
        [DefaultValue(0), DisplayName("Sort Order")]
        public int SortOrder { get; set; }
        [DisplayName("Extension Provider")]
        public string ExtensionProvider { get; set; }
    }
}
