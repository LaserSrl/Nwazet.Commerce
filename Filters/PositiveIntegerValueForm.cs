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
    public class PositiveIntegerValueForm : IFormProvider {
        public const string FormName = "PositiveIntegerValueForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public PositiveIntegerValueForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
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
                            Description: T("Enter the value."))
                        );
                    
                    return f;
                };

            context.Form(FormName, form);
        }

        public static int ParseStateValue(string sValue) {
            int value;
            if (int.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                return value;
            }
            return -1;
        }
    }
    public class PositiveIntegerValueFormValidator : IFormEventHandler {

        public Localizer T { get; set; }

        public PositiveIntegerValueFormValidator() {
            T = NullLocalizer.Instance;
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != PositiveIntegerValueForm.FormName) {
                return;
            }

            var attemptedValue = context
                .ValueProvider.GetValue("Value").AttemptedValue
                .Trim();
            // name of header to test is required
            if (string.IsNullOrWhiteSpace(attemptedValue)) {
                context.ModelState
                    .AddModelError("Value",
                        T("Value must be a positive integer (it is allowed to be a token).").Text);
            }

            // test the value
            if (PositiveIntegerValueForm.ParseStateValue(attemptedValue) <= 0) {
                // check if it looks like a token
                if (!attemptedValue.StartsWith("{") || !attemptedValue.EndsWith("}")) {
                    context.ModelState
                        .AddModelError("Value",
                            T("Value must be a positive integer (it is allowed to be a token).").Text);
                }
            }
        }

        #region Methods not implemented
        public void Building(BuildingContext context) { }

        public void Built(BuildingContext context) { }

        public void Validated(ValidatingContext context) { }
        #endregion
    }
}
