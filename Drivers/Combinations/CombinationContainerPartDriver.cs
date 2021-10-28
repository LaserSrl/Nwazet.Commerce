using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Settings.Combinations;
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
    public class CombinationContainerPartDriver : ContentPartDriver<CombinationContainerPart> {

        private readonly IProductAttributeAdminServices _productAttributeAdminServices;

        public CombinationContainerPartDriver(
            IProductAttributeAdminServices productAttributeAdminServices) {

            _productAttributeAdminServices = productAttributeAdminServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "CombinationContainerPart"; }
        }

        protected override DriverResult Editor(CombinationContainerPart part, dynamic shapeHelper) {
            var vm = CreateVM(part);
            return EditorShape(vm, shapeHelper);
        }

        protected override DriverResult Editor(CombinationContainerPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = CreateVM(part);
            return EditorShape(vm, shapeHelper);
        }

        private DriverResult EditorShape(CombinationContainerPartEditViewModel vm, dynamic shapeHelper) {
            return ContentShape("Parts_CombinationContainerPart_Editor",
                () => {
                    // TODO: handle the case where the Part is being created to avoid 
                    // messing cases with Id == 0
                    // get list of attributes we'll be able to use for combinations
                    var allAttributes = _productAttributeAdminServices
                        .GetAllProductAttributeParts();
                    vm.AllAttributeParts = allAttributes;

                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/CombinationContainerPart",
                        Model: vm,
                        Prefix: Prefix
                        );
                });
        }

        private CombinationContainerPartEditViewModel CreateVM(CombinationContainerPart part) {
            var partSettings = part.TypePartDefinition.Settings.GetModel<CombinationContainerPartSettings>();
            return new CombinationContainerPartEditViewModel() {
                Part = part,
                CombinationTypeName = partSettings?.CombinationTypeName ?? string.Empty
            };
        }
    }
}
