using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Attributes")]
    public class ProductAttributePart : ContentPart<ProductAttributePartRecord> {
        // TODO: attribute values should become their own records
        public IEnumerable<ProductAttributeValue> AttributeValues
        {
            get
            {
                return this.Record?.AttributeValueRecords
                    .Select(r => new ProductAttributeValue {
                        Text = r.Text,
                        ExtensionProvider = r.ExtensionProvider,
                        IsLineAdjustment = r.IsLineAdjustment,
                        PriceAdjustment = r.PriceAdjustment,
                        SortOrder = r.SortOrder
                    }) ?? Enumerable.Empty<ProductAttributeValue>();
            }
            set
            {
                Record.AttributeValueRecords = value
                    .Select(r => new ProductAttributeValueRecord {
                        Text = r.Text,
                        ExtensionProvider = r.ExtensionProvider,
                        IsLineAdjustment = r.IsLineAdjustment,
                        PriceAdjustment = r.PriceAdjustment,
                        SortOrder = r.SortOrder,
                        AttributePartRecord = Record
                    }).ToList();
            }
        }

        [DisplayName("Sort Order")]
        public int SortOrder
        {
            get { return Retrieve(r => r.SortOrder); }
            set { Store(r => r.SortOrder, value); }
        }

        [DisplayName("Display Name")]
        public string DisplayName
        {
            get { return Retrieve(r => r.DisplayName); }
            set { Store(r => r.DisplayName, value); }
        }

        [Required]
        [DisplayName("Technical Name")]
        public string TechnicalName
        {
            get { return Retrieve(r => r.TechnicalName); }
            set { Store(r => r.TechnicalName, value); }
        }

        public string CssName {
            get { return Retrieve(r => r.CssName); }
            set { Store(r => r.CssName, value); }
        }

        public string Meaning {
            get { return Retrieve(r => r.Meaning); }
            set { Store(r => r.Meaning, value); }
        }

        //public IList<ProductAttributeValueRecord> AttributeValueRecords {
        //    get { return Retrieve(r => r.AttributeValueRecords); }
        //    set { Store(r => r.AttributeValueRecords, value); }
        //}

        internal string AttributeValuesString
        {
            get
            {
                return Retrieve(r => r.AttributeValues);
            }
            set
            {
                Store(r => r.AttributeValues, value);
            }
        }
    }
}
