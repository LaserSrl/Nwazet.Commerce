using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Attributes")]
    public class ProductAttributeValueRecord {
        public virtual int Id { get; set; } //Primary Key
        [Required, DisplayName("Value Name")]
        public virtual string Text { get; set; }
        [Required, DisplayName("Price Adjustment")]
        public virtual decimal PriceAdjustment { get; set; }
        [DefaultValue(false), DisplayName("Is Line Adjustment")]
        public virtual bool IsLineAdjustment { get; set; }
        [DefaultValue(0), DisplayName("Sort Order")]
        public virtual int SortOrder { get; set; }
        [DisplayName("Extension Provider")]
        public virtual string ExtensionProvider { get; set; }
        public virtual ProductAttributePartRecord AttributePartRecord { get; set; } 
    }
}