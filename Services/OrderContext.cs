using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public class OrderContext {
        public IWorkContextAccessor WorkContextAccessor { get; set; }
        public IShoppingCart ShoppingCart { get; set; }
        public ICharge Charge { get; set; }
        
    }
}
