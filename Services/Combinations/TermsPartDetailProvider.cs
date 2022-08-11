using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Taxonomies.Models;

namespace Nwazet.Commerce.Services.Combinations {
    public class TermsPartDetailProvider : BaseCombinationDetailProvider {
        public TermsPartDetailProvider(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager) {

        }

        public override void AfterLoaded(CombinationPart part) {
            // If CombinationPart ContentItem has no TermsPart but container does, weld the TermsPart on to the CombinationPart ContentItem.
            if (!part.Has<TermsPart>()) {
                var container = part.CombinationContainerPart?.ContentItem;
                if (container != null) {
                    var containerTermsPart = container.As<TermsPart>();
                    if (containerTermsPart != null) {
                        part.ContentItem.Weld(containerTermsPart);
                    }
                }
            } else {
                var container = part.CombinationContainerPart?.ContentItem;
                if (container != null) {
                    var containerTermsPart = container.As<TermsPart>();
                    if (containerTermsPart != null) {
                        // Both Combination and container have the TermsPart.
                        // TermParts need to be loaded with both loaders, which aren't going to be lazy anymore.
                        // This causes a possibly relevant performance hit.
                        // TODO: join both TermsPart.TermParts.
                    }
                }
            }
        }
    }
}
