using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Filters {

    public class ProductDiscountPriceFilter : BaseDecimalFilterProvider<ProductPartVersionRecord> {
        protected override string FilterCategory => "ProductPart";
        protected override LocalizedString FilterName => T("Product");
        protected override LocalizedString FilterDescription => T("Product");
        protected override string FilterElementType => "ProductDiscountPrice";
        protected override LocalizedString FilterElementName => T("Product DiscountPrice");
        protected override LocalizedString FilterElementDescription => T("Product DiscountPrice Filter.");
        protected override string FormName => "ProductDiscountPriceFilterForm";
        protected override string PropertyName => "DiscountPrice";
        protected override string DisplayProperty => "Product DiscountPrice";

        public ProductDiscountPriceFilter() : base() { }
    }
}