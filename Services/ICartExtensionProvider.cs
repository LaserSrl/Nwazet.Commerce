using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface ICartExtensionProvider : IDependency {
        /// <summary>
        /// Return shapes that should be added to the shapes for the shopping cart
        /// </summary>
        /// <returns></returns>
        IEnumerable<dynamic> CartExtensionShapes();
    }
}
