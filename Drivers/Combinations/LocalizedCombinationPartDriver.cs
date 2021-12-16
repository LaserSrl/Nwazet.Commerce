using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class LocalizedCombinationPartDriver : ContentPartDriver<CombinationPart> {
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly ILocalizationService _localizationService;

        public LocalizedCombinationPartDriver(
            IProductAttributeAdminServices productAttributeAdminServices,
            ILocalizationService localizationService) {

            _productAttributeAdminServices = productAttributeAdminServices;
            _localizationService = localizationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

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
            var allAttributes = _productAttributeAdminServices
               .GetAllProductAttributeParts();
            var vm = new CombinationPartEditViewModel {
                Part = part,
                CombinationContainer = part.CombinationContainerPart,
                AllAttributeParts = allAttributes
            };

            // get list of attributes we'll be able to use for combinations
            if (vm.Part.ContentItem.As<LocalizationPart>() != null && vm.Part.ContentItem.As<LocalizationPart>().Culture != null) {
                string culture = vm.Part.ContentItem.As<LocalizationPart>().Culture.Culture;
                vm.AllAttributeParts = allAttributes
                    .Where(a => _localizationService.GetContentCulture(a.ContentItem) == culture);
            }

            var shapes = new List<DriverResult>();
            Func<dynamic> localizedFactory = () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/LocalizedCombinationPart",
                    Model: vm,
                    Prefix: Prefix
                    );
            };
            Func<dynamic> attributesFactory = () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/AttributesCombinationPart",
                    Model: vm,
                    Prefix: Prefix
                    );
            };
            shapes.Add(ContentShape("Parts_LocalizedCombinationPart_Editor", localizedFactory));
            shapes.Add(ContentShape("Parts_AttributesCombinationPart_Editor", attributesFactory));

            return Combined(shapes.ToArray());
        }
    }
}
