using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Nwazet.Commerce.Events {
    /// <summary>
    /// Implementation of IOrderEventHandler triggering corresponding workflow activities
    /// </summary>
    public class WorkflowOrderEventHandler : IOrderEventHandler {
        private readonly IWorkflowManager _workflowManager;

        public WorkflowOrderEventHandler(
            IWorkflowManager workflowManager) {

            _workflowManager = workflowManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T;
        public ILogger Logger;

        public void OnNewOrder(OrderPart order) {
            _workflowManager.TriggerEvent(
                   "NewOrder",
                   order,
                   () => new Dictionary<string, object> {
                        {"Content", order},
                        {"Order", order}
                   });
        }

        public void OnNewPayment(OrderPart order) {
            _workflowManager.TriggerEvent(
                   "NewPayment",
                   order,
                   () => new Dictionary<string, object> {
                        {"Content", order},
                        {"Order", order}
                   });
        }

        public void OnOrderError(OrderPart order, Dictionary<string, string> errors) {

            _workflowManager.TriggerEvent(
                   "OrderError",
                   order,
                   () => {
                       var tokens = new Dictionary<string, object> {
                           { "Content", order },
                           { "Order", order }
                       };
                       foreach (var kvp in errors) {
                           // try to add the error messages to the tokens
                           var key = kvp.Key;
                           var errorMsg = kvp.Value;
                           if (!tokens.ContainsKey(key)) {
                               tokens.Add(key, errorMsg);
                           }
                           else {
                               Logger.Error(T("OnOrderError Event: Impossible to add tokens to context: duplicate key. Key: {0}. Error Message: {1}", key, errorMsg).Text);
                           }
                       }
                       return tokens;
                   });
        }

        public void OnOrderStatusChanged(OrderPart order, string previousStatus) {
            _workflowManager.TriggerEvent(
                "OrderStatusChanged",
                order,
                () => new Dictionary<string, object> {
                    {"Content", order},
                    {"Order", order},
                    {"PreviousStatus", previousStatus},
                    {"CurrentStatus", order.Status}
                });
        }

        public void OnOrderStatusChangedProduct(OrderPart order, ContentItem product, string previousStatus) {
            if (product != null) {
                _workflowManager.TriggerEvent(
                    "OrderStatusChangedProduct", 
                    product,
                    () => new Dictionary<string, object> {
                        {"Content", product},
                        {"Order", order},
                        {"PreviousStatus", previousStatus},
                        {"CurrentStatus", order.Status}
                    });
            }
            else {
                Logger.Error(T("OnOrderStatusChangedProduct Event: Attemtp to invoke event with null product.").Text);
            }
        }

        public void OnOrderTrackingUrlChanged(OrderPart order) {
            _workflowManager.TriggerEvent(
                "OrderTrackingUrlChanged", 
                order,
                () => new Dictionary<string, object> {
                    {"Content", order},
                    {"Order", order}
                });
        }


    }
}
