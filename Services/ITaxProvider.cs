using System.Collections.Generic;
using Orchard;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Services {
    public interface ITaxProvider : IDependency {
        string Name { get; }
        string ContentTypeName { get; }

        IEnumerable<ITax> GetTaxes();
    }

    
}
