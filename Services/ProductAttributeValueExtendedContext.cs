using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public class ProductAttributeValueExtendedContext {

        public ProductAttributeValueExtendedContext() {
            Culture = CultureInfo.InvariantCulture;
        }

        public ProductAttributeValueExtendedContext(string value) 
            : this() {
            Value = value;
        }
        
        public ProductAttributeValueExtendedContext(string value, string culture)
            : this(value) {
            // try to avoid this constructor in iterations and such, becuase of the try-catch
            try {
                Culture = CultureInfo.GetCultureInfo(culture);
            } catch (Exception) {
                // failed to parse a culture
                Culture = CultureInfo.InvariantCulture;
            }
        }

        public ProductAttributeValueExtendedContext(string value, CultureInfo culture)
            : this(value) {
            Culture = culture;
        }

        public string Value { get; set; }
        public CultureInfo Culture { get; set; }
    }
}
