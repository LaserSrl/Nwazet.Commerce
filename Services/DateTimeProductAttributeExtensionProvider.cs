using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AttributeExtensions")]
    public class DateTimeProductAttributeExtensionProvider 
        : IProductAttributeExtensionProvider, IProductAttributesDriver {

        // this is a provider to use DateTime attributes, but as it is right now it's only
        // actually parsing out dates (see the return value in the Serialize method).
        // The ability to have a more fine grained configuration of this requires a rework
        // and improvement of the way ProductAttributes are implemented.

        private readonly dynamic _shapeFactory;
        private readonly IWorkContextAccessor _workContextAccessor;

        public DateTimeProductAttributeExtensionProvider(
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor) {

            _shapeFactory = shapeFactory;
            _workContextAccessor = workContextAccessor;
        }

        public string Name {
            get { return "DateTimeProductAttributeExtension"; }
        }

        public string DisplayName {
            get { return "DateTime Field"; }
        }

        public string Serialize(string value, Dictionary<string, string> form, HttpFileCollectionBase files) {
            // get the culture from the dictionary
            var culture = "";
            if (form.ContainsKey("culture")) {
                culture = form["culture"];
            }
            if (string.IsNullOrWhiteSpace(culture)) {
                // try to use a default culture
                culture = _workContextAccessor?.GetContext()?.CurrentCulture;
            }
            CultureInfo cultureInfo = null;
            if (!string.IsNullOrWhiteSpace(culture)) {
                // we have a culture, so try to parse the value to an actual date
                cultureInfo = CultureInfo.GetCultureInfo(culture);
            }
            if (cultureInfo == null) {
                cultureInfo = CultureInfo.InvariantCulture;
            }
            DateTime date = DateTime.Now;
            if (DateTime.TryParse(value, cultureInfo, DateTimeStyles.None, out date)) {
                return date.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);
            }
            // This value we are returning here is wrong, meaning we were not able to parse it
            // to a Date as we hoped. However, later when the attribute is being processed as
            // the product is added to the cart the ValidateAttributes method will get this 
            // same string and fail to validate it. (The same validation is ran when the product
            // is being added to the wishlists)
            return value;
            // TODO: find a way to prevent "wrong" values to be ever returned from here, in case
            // this is ever used somewhere without further validation.
        }

        public dynamic BuildInputShape(ProductAttributePart part) {
            return _shapeFactory.DateTimeProductAttributeExtensionInput(
                ExtensionName: Name,
                Part: part,
                Prefix: "ext.productattributes.a" + part.Id);
        }
        
        public string DisplayString(ProductAttributeValueExtendedContext context) {
            DateTime date;
            if (DateTime.TryParse(context.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                return date.ToString(context.Culture.DateTimeFormat.ShortDatePattern);
            }
            return context.Value;
        }

        public dynamic BuildAdminShape(string value) {
            return _shapeFactory.DateTimeProductAttributeExtensionAdmin();
        }


        #region Implementation of IProductAttributesDriver
        public dynamic GetAttributeDisplayShape(IContent product, dynamic shapeHelper) {
            return null;
        }

        public bool ValidateAttributes(IContent product, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues) {
            DateTime date;
            foreach (var attribute in attributeIdsToValues.Values.Where(pave => Name.Equals(pave?.ExtensionProvider))) {
                // validate that the value can be parsed as a date
                if (!DateTime.TryParse(attribute.ExtendedValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}