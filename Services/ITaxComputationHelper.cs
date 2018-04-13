using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface ITaxComputationHelper : IDependency {
        /// <summary>
        /// This method does the tax computation for the given ITax object on the give TaxContext.
        /// </summary>
        /// <param name="tax"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        decimal ComputeTax(ITax tax, TaxContext context);
    }
}
