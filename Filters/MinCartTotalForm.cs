using Orchard;
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
            _valueLabel = T("Enter the value. The decimal separator is {0}", CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator);
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


    public class MinCartTotalFormValidator : IFormEventHandler {

        public MinCartTotalFormValidator() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public void Validating(ValidatingContext context) {
            if (context.FormName != MinCartTotalForm.FormName
                && context.FormName != MinCartTotalForm.FormName) {
                return;
            }

            var attemptedValue = context
                .ValueProvider.GetValue("Value").AttemptedValue
                .Trim();
            // name of header to test is required
            if (string.IsNullOrWhiteSpace(attemptedValue)) {
                context.ModelState
                    .AddModelError("Value",
                        T("Value must be a positive number.").Text);
            }

            // test the value
            if (MinCartTotalForm.ParseStateValue(attemptedValue) <= 0) {
                context.ModelState
                    .AddModelError("Value",
                        T("Value must be a positive integer.").Text);
            } 
        }

        #region Methods not implemented
        public void Building(BuildingContext context) { }

        public void Built(BuildingContext context) { }

        public void Validated(ValidatingContext context) { }
        #endregion
    }
}