using Nwazet.Commerce.Services.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Settings.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IProductCombinationService _productCombinationService;

        public CombinationContainerPartEditorEvents(
            IProductCombinationService productCombinationService) {

            _productCombinationService = productCombinationService;
        }

        private string _oldTypeName;
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "CombinationContainerPart") {
                yield break;
            }
            var settings = definition.Settings.GetModel<CombinationContainerPartSettings>();
            // memorize current name for the ContentType used for combinations
            _oldTypeName = settings?.CombinationTypeName ?? string.Empty;
            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CombinationContainerPart") {
                yield break;
            }
            var settings = new CombinationContainerPartSettings();
            if (updateModel.TryUpdateModel(settings, "CombinationContainerPartSettings", null, null)) {
                // Should we allow changing the value of this setting with impunity?
                if (settings.UpdateCombinationName || string.IsNullOrWhiteSpace(_oldTypeName)) {
                    builder
                        .WithSetting("CombinationContainerPartSettings.CombinationTypeName",
                         settings.CombinationTypeName);
                    if (!string.IsNullOrWhiteSpace(settings.CombinationTypeName)) {
                        // Create/update the named content type to make sure it has a CombinationPart
                        _productCombinationService
                            .CreateCombinationType(builder.TypeName, settings.CombinationTypeName);
                    }
                }
            }
            yield return DefinitionTemplate(settings);
        }
    }
}
