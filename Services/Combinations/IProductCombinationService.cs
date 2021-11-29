using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    public interface IProductCombinationService : IDependency {
        
        CombinationPart GetDummyCombination(CombinationContainerPart container);

        IEnumerable<CombinationPart> CreateCombinations(
            CombinationContainerPart containerPart,
            IEnumerable<IEnumerable<AttributesToCombine>> attributesToCombines);

        string CombinationDisplayText(CombinationPart combinationPart);

        void CreateCombinationType(
            string containerTypeName, string combinationTypeName);

        IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part,
            dynamic shapeHelper);
    }
}
