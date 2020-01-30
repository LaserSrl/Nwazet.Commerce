using Orchard;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IOrderStatusProvider : IDependency {
        /// <summary>
        /// A set of possible order states.
        /// </summary>
        /// <remarks>
        /// Make sure that each provider doesn't return duplicate states by itself.
        /// Take care when merging results from different providers.
        /// Status strings are compared using StringComparer.InvariantCultureIgnoreCase
        /// </remarks>
        IEnumerable<string> States { get; }
        /// <summary>
        /// Dictionary contains state with priority 
        /// and localized label
        /// </summary>
        Dictionary<OrderStatus, LocalizedString> StatusLabels { get; }
    }

    /// <summary>
    /// This abstract class simply provides the Localizer T so we don't have to add it
    /// in every implementation.
    /// </summary>
    public abstract class BaseOrderStatusProvider : IOrderStatusProvider {

        public BaseOrderStatusProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public abstract IEnumerable<string> States { get; }
        
        public abstract Dictionary<OrderStatus, LocalizedString> StatusLabels { get; }
    }

    public class OrderStatus {
        public string StatusName { get; set; }
        /// <summary>
        /// This string is saved with 1.x.x.x
        /// it is used to place states added to the order
        /// its sorting is managed as for the PlacementInfo with class FlatPositionComparer
        /// </summary>
        public string Priority { get; set; }
    }
}
