using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPartDriver : ContentPartCloningDriver<CombinationPart> {
        public CombinationPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "CombinationPart"; }
        }

        protected override DriverResult Display(CombinationPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(CombinationPart part, dynamic shapeHelper) {
            return EditorShape(part, shapeHelper);
        }

        protected override DriverResult Editor(CombinationPart part, IUpdateModel updater, dynamic shapeHelper) {
            return EditorShape(part, shapeHelper);
        }

        private DriverResult EditorShape(CombinationPart part, dynamic shapeHelper) {
            return ContentShape("Parts_CombinationPart_Editor",
                () => {
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Combinations/CombinationPart",
                        Model: new CombinationPartEditViewModel {
                            Part = part,
                            CombinationContainer = part.CombinationContainerPart
                        },
                        Prefix: Prefix
                        );
                });
        }

        //// TODO Import/Export only AttributeValues
        //protected override void Importing(CombinationPart part, ImportContentContext context) {
        //    if (context.Data.Element(part.PartDefinition.Name) == null) {
        //        return;
        //    }
        //}

        //protected override void Exporting(CombinationPart part, ExportContentContext context) {      
        //    var root = context.Element(part.PartDefinition.Name);
        //}

        protected override void Cloning(CombinationPart originalPart, CombinationPart clonePart, CloneContentContext context) {
            // clone the combination container part id
            clonePart.CombinationContainerPartField.Value = originalPart.CombinationContainerPartField.Value;
        }
    }
}
