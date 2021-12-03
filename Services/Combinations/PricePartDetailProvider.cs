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

        public override IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
           CombinationPart part, dynamic shapeHelper) {

            // add a base shape if needed
            var pricePart = part.As<PricePart>();
            if (pricePart != null) {
                var details = new List<CombinationDetailShape>();
                // We need to update the minimum and maximum order quantities.
                details.Add(new CombinationDetailShape {
                    RoleKey = "product-price-detail",
                    Shape = shapeHelper.Combinations_ProductPriceDetail(
                        ContentItem: pricePart.ContentItem,
                        ProductPart: pricePart.As<ProductPart>(),
                        PricePart: pricePart,
                        CombinationPart: part)
                });
                // Do we need to update the out of stock message?
                return details;
            }

            return Enumerable.Empty<CombinationDetailShape>();
        }
    }
}
