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
                return date.ToShortDateString();
            }
            return value;
        }

        public dynamic BuildInputShape(ProductAttributePart part) {
            return _shapeFactory.DateTimeProductAttributeExtensionInput(
                ExtensionName: Name,
                Part: part,
                Prefix: "ext.productattributes.a" + part.Id);
        }

        public string DisplayString(string value) {
            return string.Format("[{0}]", value);
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
            foreach (var attribute in attributeIdsToValues.Values.Where(pave => pave.ExtensionProvider.Equals(Name))) {
                // validate that the value can be parssed as a date
                if (!DateTime.TryParse(attribute.ExtendedValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}