using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Utility.Extensions;

namespace Nwazet.Commerce.Extensions {
    public static class AttributeNameUtilities {

        public static string VersionName(string displayName) {
            var i = displayName.Length - 1;
            while (i >= 0 && char.IsDigit(displayName, i)) {
                i--;
            }

            var substring = i != displayName.Length - 1 ? displayName.Substring(i + 1) : string.Empty;
            int version;

            if (int.TryParse(substring, out version)) {
                displayName = displayName.Remove(displayName.Length - substring.Length);
                version = version > 0 ? ++version : 2;
            }
            else {
                version = 2;
            }

            return displayName + version;
        }

        public static string GenerateAttributeTechnicalName(ProductAttributePart part, IEnumerable<ProductAttributePart> partsToCheck) {
            return GenerateAttributeTechnicalName(part.DisplayName.ToSafeName(), partsToCheck);
        }

        public static string GenerateAttributeTechnicalName(string tName, IEnumerable<ProductAttributePart> partsToCheck) {
            tName = tName.ToSafeName();
            while (partsToCheck.Any(eap =>
                    string.Equals(eap.TechnicalName.Trim(), tName.Trim(), StringComparison.OrdinalIgnoreCase))) {
                tName = AttributeNameUtilities.VersionName(tName);
            }
            return tName;
        }

        public static string AttributesDisplayText(
            IDictionary<int, ProductAttributeValueExtended> productAttributes,
            IContent product,
            IEnumerable<IProductAttributeExtensionProvider> attributeExtensionProviders = null,
            string separator = " ") {
            var additionalText = "";
            if (productAttributes != null && productAttributes.Any()) {
                var cultureInfo = CultureInfo.InvariantCulture;
                // try to get the culture for the product content item
                if (product != null) {
                    var productLocPart = product.As<LocalizationPart>();
                    if (productLocPart != null) {
                        var culture = productLocPart.Culture != null
                            ? productLocPart.Culture.Culture : "";
                        try {
                            cultureInfo = CultureInfo.GetCultureInfo(culture);
                        } catch (Exception) {
                            cultureInfo = CultureInfo.InvariantCulture;
                        }
                    }
                }
                
                foreach (var key in productAttributes.Keys) {
                    var attributeValue = productAttributes[key];
                    if (attributeValue != null) {
                        // contribute to displayText
                        //attributeValue.Value // "Data della visita"
                        //attributeValue.ExtendedValue // "06/03/2021"
                        //attributeValue.ExtensionProvider // "DateTimeProductAttributeExtension"
                        var provider = attributeExtensionProviders != null
                            ? attributeExtensionProviders
                                .FirstOrDefault(p => p.Name.Equals(attributeValue.ExtensionProvider))
                            : attributeValue.ExtensionProviderInstance;
                        var extendedValue = attributeValue.ExtendedValue ?? "";
                        if (provider != null) {
                            extendedValue = provider.DisplayString(
                                new ProductAttributeValueExtendedContext(extendedValue, cultureInfo));
                        }
                        additionalText = string.Join(separator,
                            additionalText.Trim(),
                            attributeValue.Value.Trim() + " " + extendedValue.Trim());
                    }
                }
            }
            return additionalText;
        }
    }
}
