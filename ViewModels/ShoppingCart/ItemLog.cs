using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.ShoppingCart {
    public class ItemLog {
        public string Message { get { return LocalizedMessage.Text; } }
        public LocalizedString LocalizedMessage{ get; set; }
        public string LocalizedBaseMessage { get; set; }
        public string ProductTitle { get; set; }
        public ProductActionOptions ProductAction { get; set; }
        public int MovedQuantityAttemped { get; set; }
        public ProductResultedActionOptions ProductResultedAction { get; set; }
        public int MovedQuantity { get; set; }
    }

    public enum ProductActionOptions {
        Added, Removed
    }

    public enum ProductResultedActionOptions {
        IncreasedToMatchMinimum, DecreasedToMatchInventory, DecreasedToMatchMaximum, AddedToCart, RemovedFromCart,
        Error
    }
}
