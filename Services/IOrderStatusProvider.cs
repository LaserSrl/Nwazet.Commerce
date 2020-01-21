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
        /// </remarks>
        IEnumerable<string> States { get; }
        /// <summary>
        /// Localized labels for the different states.
        /// </summary>
        /// <remarks>
        /// These dictionaries should be handled carefully, because they can't be
        /// merged without first checking for duplicate keys.
        /// </remarks>
        Dictionary<string, LocalizedString> StatusLabels { get; }
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

        public abstract Dictionary<string, LocalizedString> StatusLabels { get; }
    }
}
