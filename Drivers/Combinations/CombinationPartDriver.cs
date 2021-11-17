using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPartDriver : ContentPartDriver<CombinationPart> {

        public CombinationPartDriver() {

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "CombinationPart"; }
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
    }
}
