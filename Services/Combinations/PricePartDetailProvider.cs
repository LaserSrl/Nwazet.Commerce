using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
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
    public class PricePartDetailProvider : 
        BaseCombinationDetailProvider {
        public PricePartDetailProvider(
           IContentDefinitionManager contentDefinitionManager)
           : base(contentDefinitionManager) {
        }

        public override ContentTypeDefinitionBuilder AlterCombinationDefinition(
            ContentTypeDefinitionBuilder previous,
            string containerTypeName) {

            var containerDefinition = _contentDefinitionManager.GetTypeDefinition(containerTypeName);

            if (containerDefinition != null
                && containerDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "PricePart")) {
                // alter the definition
                return previous.WithPart("PricePart");
            }
            return previous;
        }

        public override void Synchronize(
           CombinationContainerPart container, CombinationPart combination) {

            var sourcePart = container.As<PricePart>();
            var targetPart = combination.As<PricePart>();
            if (sourcePart != null && targetPart != null) {
                // Copy properties from the contaoiner to the combination.
                targetPart.EffectiveUnitPrice = sourcePart.EffectiveUnitPrice;
            }
        }
    }
}
