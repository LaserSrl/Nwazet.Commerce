using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public abstract class BaseCombinationDetailProvider : ICombinationDetailProvider {
        protected readonly IContentDefinitionManager _contentDefinitionManager;

        public BaseCombinationDetailProvider (
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;
        }

        public virtual ContentTypeDefinitionBuilder AlterCombinationDefinition(
            ContentTypeDefinitionBuilder previous,
            string containerTypeName) {
            // base implementation won't alter the definition
            return previous;
        }

        public virtual IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part,
            dynamic shapeHelper) {

            return Enumerable.Empty<CombinationDetailShape>();
        }
    }
}
