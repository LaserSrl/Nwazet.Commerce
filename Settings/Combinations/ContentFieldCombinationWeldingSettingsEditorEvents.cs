using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Settings.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ContentFieldCombinationWeldingSettingsEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private bool _typeHasCombinationContainer { get; set; }

        public ContentFieldCombinationWeldingSettingsEditorEvents(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;

            _typeHasCombinationContainer = false;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (_typeHasCombinationContainer) {
                var settings = definition.Settings.GetModel<ContentFieldCombinationWeldingSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.ContentTypeDefinition.Parts
                .Any(p => p.PartDefinition.Name == "CombinationContainerPart")) {
                _typeHasCombinationContainer = true;
                yield break;
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (_typeHasCombinationContainer) {
                var settings = new ContentFieldCombinationWeldingSettings();
                if (updateModel.TryUpdateModel(settings, "ContentFieldCombinationWeldingSettings", null, null)) {
                    ContentFieldCombinationWeldingSettings.SetValues(builder, settings.WeldToCombination);
                }

                yield return DefinitionTemplate(settings);
            }
        }
    }
}
