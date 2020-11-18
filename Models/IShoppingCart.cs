using System.Collections.Generic;
using Nwazet.Commerce.ViewModels;
using Orchard;

namespace Nwazet.Commerce.Models {
    public interface IShoppingCart : IDependency {
        IEnumerable<ShoppingCartItem> Items { get; }
        string Country { get; set; }
        string ZipCode { get; set; }
        ShippingOption ShippingOption { get; set; }
        void Add(int productId, int quantity = 1, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues = null);
        void AddRange(IEnumerable<ShoppingCartItem> items);
        void Remove(int productId, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues = null);
        IEnumerable<ShoppingCartQuantityProduct> GetProducts();
        ShoppingCartItem FindCartItem(int productId, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues);
        void UpdateItems();
        decimal Subtotal();
        TaxAmount Taxes(decimal subTotal = 0);
        decimal Total(decimal subTotal = 0, TaxAmount taxes = null);
        double ItemCount();
        /// <summary>
        /// Clears all items from the cart
        /// </summary>
        void Clear();
        /// <summary>
        /// Clears everything from the cart.
        /// </summary>
        void ClearAll();


        List<CartPriceAlteration> PriceAlterations { get; set; }
        IEnumerable<CartPriceAlterationAmount> PriceAlterationAmounts { get; }
    }
}