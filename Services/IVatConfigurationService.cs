using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IVatConfigurationService : IDependency {

        int GetDefaultCategoryId();
    }
}
