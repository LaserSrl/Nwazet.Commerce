using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class LocalizedCombinationPartDriver : ContentPartDriver<CombinationPart> {

        public LocalizedCombinationPartDriver() {
        }

        protected override string Prefix {
            get { return "LocalizedCombinationPart"; }
        }

        protected override DriverResult Editor(CombinationPart part, dynamic shapeHelper) {
            return EditorShape(part, shapeHelper);
        }

        protected override DriverResult Editor(CombinationPart part, IUpdateModel updater, dynamic shapeHelper) {
            return EditorShape(part, shapeHelper);
        }

        private DriverResult EditorShape(CombinationPart part, dynamic shapeHelper) {
            if ((part.ProductAttributeValues == null || !part.ProductAttributeValues.Any())
                && part.ContentItem.As<LocalizationPart>() != null) {
                // shape which has a javascript inside that reads the localization from the dropdown 
                // and calls a controller to look for the attributes

                return ContentShape("Parts_LocalizedCombinationPart_Editor",
                   () => {
                       return shapeHelper.EditorTemplate(
                           TemplateName: "Parts/Combinations/LocalizedCombinationPart",
                           Model: new CombinationPartEditViewModel {
                               Part = part,
                               CombinationContainer = part.CombinationContainerPart
                           },
                           Prefix: Prefix
                           );
                   });
            }
            return null;
        }
    }
}