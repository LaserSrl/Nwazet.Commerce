using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using System.Linq;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class LocalizationPartDetailProvider :
        BaseCombinationDetailProvider {
        public LocalizationPartDetailProvider(
           IContentDefinitionManager contentDefinitionManager)
           : base(contentDefinitionManager) {
        }
        //public override ContentTypeDefinitionBuilder AlterCombinationDefinition(
        //   ContentTypeDefinitionBuilder previous,
        //   string containerTypeName) {

        //    var containerDefinition = _contentDefinitionManager.GetTypeDefinition(containerTypeName);

        //    if (containerDefinition != null
        //        && containerDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "LocalizationPart")) {
        //        // alter the definition
        //        return previous.WithPart("LocalizationPart");
        //    }
        //    return previous;
        //}

        public override void Synchronize(
           CombinationContainerPart container, CombinationPart combination) {

            var sourcePart = container.As<LocalizationPart>();
            var targetPart = combination.As<LocalizationPart>();
            if (sourcePart != null && targetPart != null) {
                // Copy properties from the contaoiner to the combination.
                targetPart.Culture = sourcePart.Culture;

                //targetPart.MasterContentItem
            }
        }
    }
}
