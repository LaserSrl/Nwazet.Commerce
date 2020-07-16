using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Commerce")]
    public class PricePartHandler : ContentHandler {

        public PricePartHandler(
            IRepository<PricePartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));

            OnUpdated<ProductPart>((ctx, productPart) => {
                var pricePart = productPart.As<PricePart>();
                if (pricePart != null) {
                    // TODO: use providers or service to manipulate this value
                    // for example taxes, promotions and such.
                    pricePart.EffectiveUnitPrice =
                        productPart.DiscountPrice > 0 && productPart.DiscountPrice < productPart.Price
                            ? productPart.DiscountPrice
                            : productPart.Price;
                }
            });
        }

        protected override void Activating(ActivatingContentContext context) {
            // attach the PricePart wherever we have a ProductPart
            if (context.Definition.Parts.Any(ctpd => ctpd.PartDefinition.Name.Equals("ProductPart"))) {
                context.Builder.Weld<PricePart>();
            }
            base.Activating(context);
        }


    }
}
