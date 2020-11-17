using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    /// <summary>
    /// This is another version of the interface whose implementations provide
    /// additional stuff to be displayed alongside the basic order information.
    /// The idea is that implementations of this are specific for visualization
    /// on the front-end (i.e. when a user sees their own order).
    /// </summary>
    public interface IOrderFrontEndAdditionalInformationProvider : IOrderAdditionalInformationProvider {
    }
}
