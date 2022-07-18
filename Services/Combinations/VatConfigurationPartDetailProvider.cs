using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
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
    // added feature Nwazet.AdvancedVAT explicitly to ensure this provider is executed in the
    // right dependency order.
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

        public override void Synchronize(
            CombinationContainerPart container, CombinationPart combination) {

            var sourcePart = container.As<ProductVatConfigurationPart>();
            var targetPart = combination.As<ProductVatConfigurationPart>();
            if (sourcePart != null && targetPart != null) {
                // Copy properties from the container to the combination.
                targetPart.Record.VatConfiguration = sourcePart.Record.VatConfiguration;
            }
        }


        public override IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part, dynamic shapeHelper) {

            // add a base shape if needed
            var productVatConfigurationPart = part.As<ProductVatConfigurationPart>();
            if (productVatConfigurationPart != null) {
                var details = new List<CombinationDetailShape>();
                // We need to update the minimum and maximum order quantities.
                details.Add(new CombinationDetailShape {
                    RoleKey = "product-vat-configuration",
                    Shape = shapeHelper.Combinations_ProductVatConfiguration(
                        ContentItem: productVatConfigurationPart.ContentItem,
                        ProductPart: productVatConfigurationPart.As<ProductPart>(),
                        ProductVatConfigurationPart: productVatConfigurationPart,
                        CombinationPart: part)
                });
                // Do we need to update the out of stock message?
                return details;
            }

            return Enumerable.Empty<CombinationDetailShape>();
        }
    }
}
