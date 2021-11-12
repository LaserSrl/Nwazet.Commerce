using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using Orchard.ContentManagement.MetaData.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    public interface ICombinationDetailProvider : IDependency {

        ContentTypeDefinitionBuilder AlterCombinationDefinition(
           ContentTypeDefinitionBuilder previous,
           string containerTypeName);

        IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part,
            dynamic shapeHelper);
    }
}
