using System.Collections.Generic;
using System.Xml.Linq;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Localization;

namespace Nwazet.Commerce.Services {
    public interface IOrderService : IDependency {
        OrderPart CreateOrder(
            ICharge charge,
            IEnumerable<CheckoutItem> items,
            decimal subTotal,
            decimal total,
            TaxAmount taxes,
            ShippingOption shippingOption,
            Address shippingAddress,
            Address billingAddress,
            string customerEmail,
            string customerPhone,
            string specialInstructions,
            string status,
            string trackingUrl = null,            
            bool isTestOrder = false,
            int userId = -1,
            decimal amountPaid = 0,
            string purchaseOrder = "",
            string currencyCode = "",
            IEnumerable<XElement> additionalElements = null);

        OrderPart Get(int orderId);
        string GetDisplayUrl(OrderPart order);
        string GetEditUrl(OrderPart order);

        IDictionary<OrderStatus, LocalizedString> StatusLabels { get; }
        IDictionary<string, LocalizedString> EventCategoryLabels { get; }
    }
}
