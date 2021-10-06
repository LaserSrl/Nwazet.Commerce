using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Drivers;
using System;

namespace Nwazet.Commerce.Drivers {
    public class ShoppingCartWidgetDriver : ContentPartDriver<ShoppingCartWidgetPart>
    {
        private readonly IShoppingCart _shoppingCart;

        public ShoppingCartWidgetDriver(IShoppingCart shoppingCart) {
            _shoppingCart = shoppingCart;
        }

        protected override DriverResult Display(ShoppingCartWidgetPart part, string displayType, dynamic shapeHelper) 
        {
            return ContentShape("ShoppingCartWidget", () => shapeHelper.ShoppingCartWidget(
                ItemCount: (Func<double>)_shoppingCart.ItemCount,
                TotalAmount: (Func<decimal>)(() => _shoppingCart.Total()),
                ContentItem: part.ContentItem
            ));
        }
    }
}