using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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


        public static string GenerateUniqueKey(this CheckoutItem item) {
            var key = "";
            // We should be accounting for product attributes:
            //  - having different attributes means we may have multiple lines in the order for a product with
            //    the same Id.
            //  - It means that the way this data is stored has to be adapted to accomodate for it.
            //  - While they would have the same VAT Rate (at least for now), those multiple lines could in
            //    principle have different prices
            //  - That means that the product's Id is not enough of a key
            if (item.Attributes == null || !item.Attributes.Any()) {
                // this is like this for retrocompatibility with the time attributes were
                // not considered correctly.
                key = item.ProductId.ToString();
            } else {
                // there are attributes
                var keyStruct = new KeyFormat {
                    ProductId = item.ProductId,
                    Attributes = item.Attributes as Dictionary<int, ProductAttributeValueExtended>
                };

                key = JsonConvert.SerializeObject(keyStruct, Formatting.None);
            }
            return key;
        }

        public static string GenerateUniqueKey(this ShoppingCartQuantityProduct item) {
            var key = "";
            // We should be accounting for product attributes:
            //  - having different attributes means we may have multiple lines in the order for a product with
            //    the same Id.
            //  - It means that the way this data is stored has to be adapted to accomodate for it.
            //  - While they would have the same VAT Rate (at least for now), those multiple lines could in
            //    principle have different prices
            //  - That means that the product's Id is not enough of a key
            if (item.AttributeIdsToValues == null || !item.AttributeIdsToValues.Any()) {
                // this is like this for retrocompatibility with the time attributes were
                // not considered correctly.
                key = item.Product.Id.ToString();
            } else {
                // there are attributes
                var keyStruct = new KeyFormat {
                    ProductId = item.Product.Id,
                    Attributes = item.AttributeIdsToValues as Dictionary<int, ProductAttributeValueExtended>
                };

                key = JsonConvert.SerializeObject(keyStruct, Formatting.None);
            }
            return key;
        }

        struct KeyFormat {
            public int ProductId { get; set; }
            public Dictionary<int, ProductAttributeValueExtended> Attributes { get; set; }
        }
    }
}
