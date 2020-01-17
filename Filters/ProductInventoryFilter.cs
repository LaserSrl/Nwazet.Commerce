using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Filters {

    public class ProductInventoryFilter : BaseDecimalFilterProvider<InventoryPartRecord> {
        protected override string FilterCategory => "InventoryPart";
        protected override LocalizedString FilterName => T("Inventory");
        protected override LocalizedString FilterDescription => T("Inventory");
        protected override string FilterElementType => "ProductInventory";
        protected override LocalizedString FilterElementName => T("Product Inventory");
        protected override LocalizedString FilterElementDescription => T("Product Inventory Filter.");
        protected override string FormName => "ProductInventoryFilterForm";
        protected override string PropertyName => "Inventory";
        protected override string DisplayProperty => "Product Inventory";

        public ProductInventoryFilter() : base() { }
    }
}