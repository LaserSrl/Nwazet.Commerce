using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    public interface ICombinationStatusProvider : IDependency {

        List<CombinationStatusMessage> CombinationStatus(
            CombinationContainerPart container,CombinationPart combination);
    }
}
