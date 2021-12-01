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
        public virtual string GUIdentifier { get; set; }
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

        public static IEnumerable<ProductAttributeValueRecord> DeserializeAttributeValues(string attributeValues) {
            if (attributeValues != null) {
                return attributeValues.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Split('=')).Select(av => {
                        var attrSettings = av[1].Split(',');
                        return new ProductAttributeValueRecord {
                            Text = av[0],
                            PriceAdjustment = Convert.ToDecimal(attrSettings[0]),
                            IsLineAdjustment = Convert.ToBoolean(attrSettings[1]),
                            // Check if sort order value is present, didn't exist in previous versions
                            SortOrder = attrSettings.Length > 2 ? Convert.ToInt32(attrSettings[2]) : 0,
                            // Check if extension provider value is present, didn't exist in previous versions
                            ExtensionProvider = attrSettings.Length > 3 ? attrSettings[3] : string.Empty,
                            GUIdentifier = attrSettings[4]
                        };
                    })
                    .OrderBy(a => a.SortOrder)
                    .ThenBy(a => a.Text)
                    .ToList();
            } else {
                return new List<ProductAttributeValueRecord>();
            }
        }

        public static string SerializeAttributeValues(IEnumerable<ProductAttributeValueRecord> attributeValues) {
            if (attributeValues != null) {
                return string.Join(";", attributeValues.Select(a => a.Text + "=" + a.PriceAdjustment + ","
                    + a.IsLineAdjustment + "," + a.SortOrder + "," + a.ExtensionProvider + ","+a.GUIdentifier));
            } else {
                return string.Empty;
            }
        }
    }
}