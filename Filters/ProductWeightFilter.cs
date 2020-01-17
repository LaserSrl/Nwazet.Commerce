using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Filters {

    public class ProductWeightFilter : BaseDecimalFilterProvider<ProductPartVersionRecord> {
        protected override string FilterCategory => "ProductPart";
        protected override LocalizedString FilterName => T("Product");
        protected override LocalizedString FilterDescription => T("Product");
        protected override string FilterElementType => "ProductWeight";
        protected override LocalizedString FilterElementName => T("Product Weight");
        protected override LocalizedString FilterElementDescription => T("Product Weight Filter.");
        protected override string FormName => "ProductWeightFilterForm";
        protected override string PropertyName => "Weight";
        protected override string DisplayProperty => "Product Weight";

        public ProductWeightFilter() : base() { }
    }
}