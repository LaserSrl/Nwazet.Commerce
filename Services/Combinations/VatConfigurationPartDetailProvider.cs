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
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPartDetailProvider :
        BaseCombinationDetailProvider {
        public VatConfigurationPartDetailProvider(
           IContentDefinitionManager contentDefinitionManager)
           : base(contentDefinitionManager) {
        }

        public override ContentTypeDefinitionBuilder AlterCombinationDefinition(
           ContentTypeDefinitionBuilder previous,
           string containerTypeName) {

            var containerDefinition = _contentDefinitionManager.GetTypeDefinition(containerTypeName);

            if (containerDefinition != null
                && containerDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProductVatConfigurationPart")) {
                // alter the definition
                return previous.WithPart("ProductVatConfigurationPart");
            }
            return previous;
        }
    }
}
