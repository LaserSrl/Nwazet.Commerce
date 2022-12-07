using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Events {
    public interface IOrderEventHandler : IEventHandler {
        void OnNewOrder(OrderPart order);
        void OnNewPayment(OrderPart order);
        void OnOrderError(OrderPart order, Dictionary<string,string> errors);

        void OnOrderStatusChanged(OrderPart order, string previousStatus);
        void OnOrderTrackingUrlChanged(OrderPart order);
        void OnOrderStatusChangedProduct(OrderPart order, ContentItem product, string previousStatus);
    }
}
