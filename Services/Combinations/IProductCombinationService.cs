using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using System.Collections.Generic;

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

        string GetCombinationContentType(
            CombinationContainerPart containerPart);
    }
}
