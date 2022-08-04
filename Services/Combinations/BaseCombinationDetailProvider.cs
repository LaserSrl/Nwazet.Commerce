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
            // Base implementation won't alter the definition
            return previous;
        }

        public virtual void Synchronize(
            CombinationContainerPart container, CombinationPart combination) {
            // Base implementation empty on purpose
        }

        public virtual IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part,
            dynamic shapeHelper) {
            // Base implementation empty on purpose
            return Enumerable.Empty<CombinationDetailShape>();
        }
    }
}
