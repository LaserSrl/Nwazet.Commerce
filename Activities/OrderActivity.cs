using System.Collections.Generic;
using System.Linq;
using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Nwazet.Commerce.Activities {
    [OrchardFeature("Nwazet.Orders")]
    public abstract class OrderActivity : Event {

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext,
            ActivityContext activityContext) {
            return new[] {T("Done")};
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext,
            ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Commerce"); }
        }
    }

    [OrchardFeature("Nwazet.Orders")]
    public class NewOrder : OrderActivity {
        public override string Name {
            get { return "NewOrder"; }
        }

        public override LocalizedString Description {
            get { return T("A new order has been passed."); }
        }
    }

    [OrchardFeature("Nwazet.Orders")]
    public class NewPayment : OrderActivity {
        public override string Name {
            get { return "NewPayment"; }
        }

        public override LocalizedString Description {
            get { return T("A new payment has been made."); }
        }
    }

    [OrchardFeature("Nwazet.Orders")]
    public class OrderError : OrderActivity {
        public override string Name {
            get { return "OrderError"; }
        }

        public override LocalizedString Description {
            get { return T("An order resulted in an error."); }
        }
    }

    [OrchardFeature("Nwazet.Orders")]
    public class OrderStatusChanged : OrderActivity {
        public override string Name {
            get { return "OrderStatusChanged"; }
        }

        public override LocalizedString Description {
            get { return T("The status of an order has changed."); }
        }

        public override string Form {
            get {
                return "SelectOrderStatus";
            }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {
                var statusState = activityContext.GetState<string>("OrderStatus");

                if (string.IsNullOrEmpty(statusState)) {
                    return true;
                }

                string[] selectedStatus = statusState.Split(',');

                var content = workflowContext.Content;
                if (content == null) {
                    return false;
                }

                return selectedStatus.Any(s => ((OrderPart)content).Status == s);
            } catch {
                return false;
            }
        }
    }

    [OrchardFeature("Nwazet.Orders")]
    public class OrderTrackingUrlChanged : OrderActivity {
        public override string Name {
            get { return "OrderTrackingUrlChanged"; }
        }

        public override LocalizedString Description {
            get { return T("The tracking URL of an order has changed."); }
        }
    }

    [OrchardFeature("Nwazet.Orders")]
    public class OrderStatusChangedProduct : OrderActivity {
        public override string Name {
            get { return "OrderStatusChangedProduct"; }
        }

        public override LocalizedString Description {
            get { return T("Triggered for each product in the order when the status changes."); }
        }

        public override string Form {
            get {
                return "SelectOrderStatus";
            }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {
                var statusState = activityContext.GetState<string>("OrderStatus");

                if (string.IsNullOrEmpty(statusState)) {
                    return true;
                }

                string[] selectedStatus = statusState.Split(',');

                if (!workflowContext.Tokens.ContainsKey("Order")) {
                    return false;
                }

                var content = workflowContext.Tokens["Order"];
                if (content == null) {
                    return false;
                }

                return selectedStatus.Any(s => ((OrderPart)content).Status == s);
            } catch {
                return false;
            }
        }
    }
}