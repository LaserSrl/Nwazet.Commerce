using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    /// <summary>
    /// Instances of this class represent additional information being added to each product
    /// in an order.
    /// </summary>
    public class OrderEditorAdditionalProductInfoViewModel {
        /// <summary>
        /// The title used as column name for the information.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The css class for the element that will represent the header for this
        /// information.
        /// </summary>
        public string HeaderClass { get; set; }

        /// <summary>
        /// This Dictionary contains the information we wish to add.
        /// In an order, there may be more than one item for the same Product ContentItem when
        /// attributes are involved. That means the key for the Information Dictionary has to be
        /// more complex than just that ContentItem's Id
        /// </summary>
        public Dictionary<string, string> Information { get; set; }
        /// <summary>
        /// The css class for the element that will represent the information.
        /// </summary>
        public string InformationClass { get; set; }
    }
}
