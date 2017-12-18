using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IVatConfigurationService : IDependency {

        /// <summary>
        /// Gets the Id of the product category that is set as the default one to be used. The default
        /// category is the product category that will be implicitly assigned to products whenever they
        /// have no category assigned to them explicitly.
        /// </summary>
        /// <returns>The Id of the default product category. The return value will be negative if no
        /// default category is set.</returns>
        /// <remarks>This method should only consider published categories.</remarks>
        int GetDefaultCategoryId();
    }
}
