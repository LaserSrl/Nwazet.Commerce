using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IUpdateStatusService : IDependency {
        void UpdateOrderStatusChanged(OrderPart part, string newStatus);
    }
}
