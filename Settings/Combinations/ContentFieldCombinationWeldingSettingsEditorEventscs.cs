using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.Settings.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ContentFieldCombinationWeldingSettingsEditorEventscs : ContentDefinitionEditorEventsBase {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentFieldCombinationWeldingSettingsEditorEventscs(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            var settings = definition.Settings.GetModel<ContentFieldCombinationWeldingSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var settings = new ContentFieldCombinationWeldingSettings();
            if (updateModel.TryUpdateModel(settings, "ContentFieldCombinationWeldingSettings", null, null)) {
                ContentFieldCombinationWeldingSettings.SetValues(builder, settings.WeldToCombination);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
