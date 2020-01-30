using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Services {
    public class DefaultOrderStatusProvider : BaseOrderStatusProvider {

        public DefaultOrderStatusProvider() : base() { }

        #region private 
        private static readonly OrderStatus pendingState =
            new OrderStatus { StatusName = OrderPart.Pending, Priority = "1.1" };

        private static readonly OrderStatus acceptedState =
            new OrderStatus { StatusName = OrderPart.Accepted, Priority = "1.2" };

        private static readonly OrderStatus archivedState =
            new OrderStatus { StatusName = OrderPart.Archived, Priority = "1.3" };

        private static readonly OrderStatus cancelledState =
            new OrderStatus { StatusName = OrderPart.Cancelled, Priority = "1.4" };
        #endregion

        public static readonly string[] states = {
            OrderPart.Pending, OrderPart.Accepted, OrderPart.Archived, OrderPart.Cancelled
        };

        public override IEnumerable<string> States => states;

        public override Dictionary<OrderStatus, LocalizedString> StatusLabels =>
            new Dictionary<OrderStatus, LocalizedString> {
                { pendingState, T("Pending") },
                { acceptedState, T("Accepted") },
                { archivedState, T("Archived") },
                { cancelledState, T("Cancelled") }
            };
    }
}
