using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
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
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        
        public LocalizedCombinationPartDriver(
            IProductAttributeAdminServices productAttributeAdminServices,
            ILocalizationService localizationService,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor) {

            _productAttributeAdminServices = productAttributeAdminServices;
            _localizationService = localizationService;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "LocalizedCombinationPart"; }
        }

        protected override DriverResult Editor(CombinationPart part, dynamic shapeHelper) {
            var vm = CreateVM(part);
            return EditorShape(vm, shapeHelper);
        }

        protected override DriverResult Editor(CombinationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = CreateVM(part);

            updater.TryUpdateModel(vm, Prefix, null, null);
            
            return EditorShape(vm, shapeHelper);
        }

        private DriverResult EditorShape(CombinationPartEditViewModel vm, dynamic shapeHelper) {
            var shapes = new List<DriverResult>();
            // shape which has a javascript inside that reads the localization from the dropdown 
            // and calls a controller to look for the attributes
            Func<dynamic> localizedFactory = () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/LocalizedCombinationPart",
                    Model: vm,
                    Prefix: Prefix
                    );
            };
            // shape showing the attributes
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

        private CombinationPartEditViewModel CreateVM(CombinationPart part) {
            var vm = new CombinationPartEditViewModel();
            // get list of attributes we'll be able to use for combinations
            var allAttributes = _productAttributeAdminServices
                .GetAllProductAttributeParts();

            vm = new CombinationPartEditViewModel {
                Part = part,
                CombinationContainer = part.CombinationContainerPart
            };

            // if the localization part is present, we select the attributes by culture
            var culture = _workContextAccessor.GetContext().CurrentCulture;
            if (vm.Part.ContentItem.As<LocalizationPart>() != null && vm.Part.ContentItem.As<LocalizationPart>().Culture != null) {
                culture = vm.Part.ContentItem.As<LocalizationPart>().Culture.Culture;
            }
            allAttributes = allAttributes
                .Where(a => _localizationService.GetContentCulture(a.ContentItem) == culture);

            vm.AllAttributes = allAttributes.Select(a =>
                new CombinationAttribute {
                    AttributeId = a.Id,
                    AttributeDisplayName = string.IsNullOrWhiteSpace(a.DisplayName) ? _contentManager.GetItemMetadata(a).DisplayText : a.DisplayName,
                    Values = a.AttributeValues.Select(v => new AttributeValue { Id = v.Id, Text = v.Text }).ToList(),
                    SelectedAttributeValue =  -1
                }).ToList();
            return vm;
        }
    }
}
