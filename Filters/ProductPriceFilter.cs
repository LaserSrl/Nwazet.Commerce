using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Filters {

    public class ProductPriceFilter : BaseDecimalFilterProvider<ProductPartVersionRecord> {
        protected override string FilterCategory => "ProductPart";
        protected override LocalizedString FilterName => T("Product");
        protected override LocalizedString FilterDescription => T("Product");
        protected override string FilterElementType => "ProductPrice";
        protected override LocalizedString FilterElementName => T("Product Price");
        protected override LocalizedString FilterElementDescription => T("Product Price Filter.");
        protected override string FormName => "ProductPriceFilterForm";
        protected override string PropertyName => "Price";
        protected override string DisplayProperty => "Product Price";

        public ProductPriceFilter() : base() { }
    }
}