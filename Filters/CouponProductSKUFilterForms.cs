using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Filters {
    public class CouponProductSkuFilterForm : IFormProvider {
        public const string FormName = "CouponProductSkuFilterForm";
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public CouponProductSkuFilterForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "CouponProductSkuFilterForm",
                        _CouponProductFilterForm: Shape.TextArea(
                            Id: "Value", Name: "Value",
                            Title: T("The Product Skus"),
                            Description: T("Product Skus. Write one SKU per line."),
                            Classes: new[] { "text", "tokenized", "required" }
                        ),
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ));

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CartCouponSKUOperator.CartContainsAllSKUs),
                        Text = T("Cart contains all the products corresponding to the SKUs").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CartCouponSKUOperator.CartContainsAnySKU),
                        Text = T("Cart contains at least one of the products corresponding to the SKUs").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CartCouponSKUOperator.CartContainsNone),
                        Text = T("Cart contains none of the products corresponding to the SKUs").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CartCouponSKUOperator.CartContainsNoOther),
                        Text = T("Cart contains only products corresponding to the SKUs").Text
                    });

                    return f;
                };
            context.Form(FormName, form);
        }

        static readonly string[] _separators =  { Environment.NewLine, "\n", "\n\r" };

        public static IEnumerable<string> ParseStateValue(dynamic state) {
            if (state == null || state.Value == null) {
                return Enumerable.Empty<string>();
            }
            return ((string)state.Value ?? string.Empty)
                .Split(_separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static LocalizedString DisplayFilter(Localizer T, dynamic state) {
            var skus = (string)string.Join(", ", ParseStateValue(state));
            var op = (CartCouponSKUOperator)Enum.Parse(typeof(CartCouponSKUOperator), Convert.ToString(state.Operator));

            switch (op) {
                case CartCouponSKUOperator.CartContainsAllSKUs:
                    return T("Cart contains all these product SKUs: {0}", skus);
                case CartCouponSKUOperator.CartContainsAnySKU:
                    return T("Cart contains at least one of these product SKUs: {0}", skus);
                case CartCouponSKUOperator.CartContainsNone:
                    return T("Cart contains none of these product SKUs: {0}", skus);
                case CartCouponSKUOperator.CartContainsNoOther:
                    return T("Cart doesn't contain any of these product SKUs: {0}", skus);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool EvaluateFilter(IEnumerable<string> cartSkus, dynamic state) {
            var skusToTest = cartSkus.Distinct();
            var skus = (IEnumerable<string>)ParseStateValue(state);
            var op = (CartCouponSKUOperator)Enum.Parse(typeof(CartCouponSKUOperator), Convert.ToString(state.Operator));
            
            switch (op) {
                // remember that skus are case sensitive
                case CartCouponSKUOperator.CartContainsAllSKUs:
                    // all the skus from the state MUST be among the skus from the cart
                    return skus.All(s => skusToTest.Contains(s));
                case CartCouponSKUOperator.CartContainsAnySKU:
                    // at least one sku from the state must be among the skus from the cart
                    return skus.Any(s => skusToTest.Contains(s));
                case CartCouponSKUOperator.CartContainsNone:
                    // no sku from the state must be among the skus from the cart
                    return !skus.Any(s => skusToTest.Contains(s));
                case CartCouponSKUOperator.CartContainsNoOther:
                    // all the skus from the cart must be among the skus from the state
                    return skusToTest.All(s => skus.Contains(s));
                default:
                    return false;
            }
        }
    }

    public class CouponSingleProductSkuFilterForm : IFormProvider {
        public const string FormName = "CouponSingleProductSkuFilterForm";
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public CouponSingleProductSkuFilterForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "CouponProductSkuFilterForm",
                        _CouponProductFilterForm: Shape.TextArea(
                            Id: "Value", Name: "Value",
                            Title: T("The Product Skus"),
                            Description: T("Product Skus. Write one SKU per line."),
                            Classes: new[] { "text", "tokenized", "required" }
                        ),
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ));

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(ProductCouponSKUOperator.ProductMatchesSKU),
                        Text = T("Product of the line corresponds to one of the SKUs").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(ProductCouponSKUOperator.ProductDoesntMatchSKU),
                        Text = T("Product of the line doesn't correspond to any of the SKUs").Text
                    });

                    return f;
                };
            context.Form(FormName, form);
        }

        static readonly string[] _separators = { Environment.NewLine, "\n", "\n\r" };

        public static IEnumerable<string> ParseStateValue(dynamic state) {
            if (state == null || state.Value == null) {
                return Enumerable.Empty<string>();
            }
            return ((string)state.Value ?? string.Empty)
                .Split(_separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static LocalizedString DisplayFilter(Localizer T, dynamic state) {
            var skus = (string)string.Join(", ", ParseStateValue(state));
            var op = (ProductCouponSKUOperator)Enum.Parse(typeof(ProductCouponSKUOperator), Convert.ToString(state.Operator));

            switch (op) {
                case ProductCouponSKUOperator.ProductMatchesSKU:
                    return T("Product of the line corresponds to one of these product SKUs: {0}", skus);
                case ProductCouponSKUOperator.ProductDoesntMatchSKU:
                    return T("Product of the line doesn't correspond to any of these product SKUs: {0}", skus);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool EvaluateFilter(string sku, dynamic state) {

            var skus = (IEnumerable<string>)ParseStateValue(state);
            var op = (ProductCouponSKUOperator)Enum.Parse(typeof(ProductCouponSKUOperator), Convert.ToString(state.Operator));

            switch (op) {
                // remember that skus are case sensitive
                case ProductCouponSKUOperator.ProductMatchesSKU:
                    return skus.Any(s => s.Equals(sku));
                case ProductCouponSKUOperator.ProductDoesntMatchSKU:
                    return !skus.Any(s => s.Equals(sku));
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
    /// <summary>
    /// Validator for both forms above
    /// </summary>
    public class CouponProductSkuFilterFormValidator : IFormEventHandler {

        public Localizer T { get; set; }

        public CouponProductSkuFilterFormValidator() {
            T = NullLocalizer.Instance;
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != CouponProductSkuFilterForm.FormName
                && context.FormName != CouponSingleProductSkuFilterForm.FormName) {
                return;
            }

            var attemptedValue = context
                .ValueProvider.GetValue("Value").AttemptedValue
                .Trim();
            // at least one sku is required
            if (string.IsNullOrWhiteSpace(attemptedValue)) {
                context.ModelState
                    .AddModelError("Value",
                        T("Value cannot be empty.").Text);
            }

            var attemptedOperator = context
                .ValueProvider.GetValue("Operator").AttemptedValue
                .Trim();
            // operator required
            if (context.FormName == CouponProductSkuFilterForm.FormName) {
                CartCouponSKUOperator op;
                if (string.IsNullOrWhiteSpace(attemptedOperator)
                    || !Enum.TryParse<CartCouponSKUOperator>(attemptedOperator, out op)) {
                    context.ModelState
                        .AddModelError("Operator",
                            T("Must select a valid operator.").Text);
                }
            } else if (context.FormName == CouponSingleProductSkuFilterForm.FormName) {
                ProductCouponSKUOperator op;
                if (string.IsNullOrWhiteSpace(attemptedOperator)
                    || !Enum.TryParse<ProductCouponSKUOperator>(attemptedOperator, out op)) {
                    context.ModelState
                        .AddModelError("Operator",
                            T("Must select a valid operator.").Text);
                }
            }

        }

        #region Methods not implemented
        public void Building(BuildingContext context) { }

        public void Built(BuildingContext context) { }

        public void Validated(ValidatingContext context) { }
        #endregion
    }

    public enum CartCouponSKUOperator {
        CartContainsAllSKUs, //
        CartContainsAnySKU, //
        CartContainsNone, // None of the products corresponding to the skus is in the cart
        CartContainsNoOther, // The SKUs of all products in the cart are in the list of skus
    }
    public enum ProductCouponSKUOperator {
        ProductMatchesSKU,
        ProductDoesntMatchSKU
    }
}
