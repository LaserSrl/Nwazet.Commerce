using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Filters {
    public class MinCartTotalForm : IFormProvider {
        public const string FormName = "MinTotalPriceValueForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }
        protected LocalizedString _valueLabel { get; set; }
        protected string _formName { get; set; }
        public MinCartTotalForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
            _valueLabel = T("Enter the value.");
            _formName = FormName;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: FormName,
                        _Value: Shape.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "text medium", "tokenized" },
                            Description: _valueLabel)
                        );

                    return f;
                };

            context.Form(_formName, form);
        }
        public static decimal ParseStateValue(string sValue) {
            decimal value;
            if (decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                return value;
            }
            return -1;
        }
    }
}