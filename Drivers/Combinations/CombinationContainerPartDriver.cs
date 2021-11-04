using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Combinations;
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
        private readonly IContentManager _contentManager;
        private readonly IProductCombinationService _productCombinationService;

        public CombinationContainerPartDriver(
            IProductAttributeAdminServices productAttributeAdminServices,
            IContentManager contentManager,
            IProductCombinationService productCombinationService) {

            _productAttributeAdminServices = productAttributeAdminServices;
            _contentManager = contentManager;
            _productCombinationService = productCombinationService;

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
                    // TODO: check user permissions
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
            // Get existing combinations
            var currentCombinationRecords = part?.Record?.CombinationPartRecords ?? Enumerable.Empty<CombinationPartRecord>();
            var combinationContents = _contentManager
                .GetMany<CombinationPart>(currentCombinationRecords.Select(cpr => cpr.Id), VersionOptions.Latest, QueryHints.Empty);
            var comboTitles = new Dictionary<int, string>();
            foreach (var combo in combinationContents) {
                comboTitles.Add(
                    combo.Id,
                    _productCombinationService.AdminDisplayText(combo));
            }
            return new CombinationContainerPartEditViewModel() {
                Part = part,
                CurrentCombinations = combinationContents,
                CombinationTitles = comboTitles,
                CombinationTypeName = partSettings?.CombinationTypeName ?? string.Empty
            };
        }
    }
}
