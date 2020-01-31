using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Orders")]
    public class OrderService : IOrderService {
        private static readonly RNGCryptoServiceProvider RngCsp = new RNGCryptoServiceProvider();

        private readonly IContentManager _contentManager;
        private readonly UrlHelper _url;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IEnumerable<IOrderAdditionalInformationProvider> _orderAdditionalInformationProviders;
        private readonly IEnumerable<IOrderStatusProvider> _orderStatusProviders;

        public OrderService(
            IContentManager contentManager,
            UrlHelper url,
            ICurrencyProvider currencyProvider,
            IEnumerable<IOrderAdditionalInformationProvider> orderAdditionalInformationProviders,
            IEnumerable<IOrderStatusProvider> orderStatusProviders) {

            _contentManager = contentManager;
            _url = url;
            _currencyProvider = currencyProvider;
            _orderAdditionalInformationProviders = orderAdditionalInformationProviders;
            _orderStatusProviders = orderStatusProviders;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string GetDisplayUrl(OrderPart order) {
            return _url.Action("Show", "OrderSsl", new { id = order.Id });
        }

        public string GetEditUrl(OrderPart order) {
            return _url.ItemEditUrl(order);
        }

        public OrderPart CreateOrder(
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
            string currencyCode = "") {

            var order = _contentManager.Create("Order", VersionOptions.DraftRequired).As<OrderPart>();
            order.Build(charge, items, subTotal, total, taxes,
                shippingOption, shippingAddress, billingAddress, customerEmail,
                customerPhone, specialInstructions,
                string.IsNullOrWhiteSpace(currencyCode) ? _currencyProvider.CurrencyCode : currencyCode,
                amountPaid, purchaseOrder);
            order.Status = status;
            order.TrackingUrl = trackingUrl;
            order.IsTestOrder = isTestOrder;
            order.UserId = userId;

            var random = new byte[8];
            RngCsp.GetBytes(random);
            order.Password = Convert.ToBase64String(random);

            _contentManager.Publish(order.ContentItem);


            // TODO: Here we have created the order, so we may use it (and its ContentItem)
            // to create additional records that will contain further information. We do this
            // through the implementations of IOrderAdditionalInformationProvider
            foreach (var oaip in _orderAdditionalInformationProviders) {
                oaip.StoreAdditionalInformation(order);
            }

            return order;
        }

        public OrderPart Get(int orderId) {
            return _contentManager.Get<OrderPart>(orderId);
        }

        public IDictionary<OrderStatus, LocalizedString> StatusLabels {
            get {
                // Depending on the order of the providers, we may end up
                // with a different dictionary when different providers have the
                // same state.
                return _orderStatusProviders
                    .Select(osp => osp.StatusLabels)
                    .Aggregate(new Dictionary<OrderStatus, LocalizedString>(),
                        (partial, candidate) => {
                            //accumulator
                            foreach (var row in candidate) {
                                if(partial.Keys.FirstOrDefault(s => s.StatusName == row.Key.StatusName)==null) {
                                    partial.Add(row.Key, row.Value);
                                }
                            }
                            return partial;
                        });
            }
        }

        public IDictionary<string, LocalizedString> EventCategoryLabels {
            get {
                return new Dictionary<string, LocalizedString> {
                    {OrderPart.Note, T("Note")},
                    {OrderPart.Warning, T("Warning")},
                    {OrderPart.Error, T("Error")},
                    {OrderPart.Task, T("Task")},
                    {OrderPart.Event, T("Event")}
                };
            }
        }
    }
}