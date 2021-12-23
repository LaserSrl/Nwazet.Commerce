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
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IProductCombinationService _productCombinationService;
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public LocalizedCombinationPartDriver(
            IOrchardServices orchardServices,
            IProductAttributeAdminServices productAttributeAdminServices,
            IProductCombinationService productCombinationService,
            ILocalizationService localizationService,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor) {

            Services = orchardServices;
            _productAttributeAdminServices = productAttributeAdminServices;
            _productCombinationService = productCombinationService;
            _localizationService = localizationService;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T;
        public IOrchardServices Services { get; private set; }

        protected override string Prefix {
            get { return "LocalizedCombinationPart"; }
        }

        protected override DriverResult Editor(CombinationPart part, dynamic shapeHelper) {
            var vm = CreateVM(part);
            return EditorShape(vm, shapeHelper);
        }

        protected override DriverResult Editor(CombinationPart part, IUpdateModel updater, dynamic shapeHelper) {
            // CombinationConfigurationAdminController.cs Create(), Creates the content of the element
            // in the driver takes care of assigning the created combination
            var vm = CreateVM(part);

            // the driver does not have to do anything 
            // if there are already assigned combinations
            if (vm.Part.ProductAttributeValues != null && !part.ProductAttributeValues.Any()) {
                return EditorShape(vm, shapeHelper);
            }

            updater.TryUpdateModel(vm, Prefix, null, null);

            // populate missing property
            var allAttributes = GetAllAttributes(vm.Part);

            for (int i = 0; i < vm.AllAttributes.Count(); i++) {
                var attribute = allAttributes.FirstOrDefault(a => a.Id == vm.AllAttributes[i].AttributeId);
                if (attribute != null) {
                    vm.AllAttributes[i].Values = attribute.AttributeValues.Select(v => new AttributeValue { Id = v.Id, Text = v.Text }).ToList();
                }
            }
            // check if an attribute has been selected
            if (!vm.AllAttributes.Any(a => a.SelectedAttributeValue != -1)) {
                updater.AddModelError("MissingCombination", T("There is no attribute selected."));
                return EditorShape(vm, shapeHelper);
            }

            // created new combination
            List<AttributesToCombine> attributeToCombine = new List<AttributesToCombine>();
            foreach (var att in vm.AllAttributes.Where(a => a.SelectedAttributeValue != -1)) {
                attributeToCombine.Add(new AttributesToCombine { AttributeId = att.AttributeId, AttributeValue = att.SelectedAttributeValue });
            }

            // check that the new combination is not already present
            var currentCombinations = part.CombinationContainerPart
                .CombinationParts
                .Select(cp => CombinationPart.DeserializeCombinations(cp));
            foreach (var current in currentCombinations) {
                if (current != null && current.Count() == attributeToCombine.Count()
                   && !attributeToCombine.Any(a =>
                    !current.Any(com => com.AttributeId == a.AttributeId && com.AttributeValue == a.AttributeValue))) {
                    updater.AddModelError("ErrorCombination", T("The selected combination already exists. Try again."));
                    return EditorShape(vm, shapeHelper);
                }
            }

            // Create the new contents
            part.ProductAttributeValues = attributeToCombine;

            Services.Notifier.Information(T("A new combination has been created"));

            return EditorShape(vm, shapeHelper);
        }

        private DriverResult EditorShape(CombinationPartEditViewModel vm, dynamic shapeHelper) {
            var shapes = new List<DriverResult>();
            if (vm.Part.ProductAttributeValues == null || !vm.Part.ProductAttributeValues.Any()) {
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
            }

            return Combined(shapes.ToArray());
        }

        private CombinationPartEditViewModel CreateVM(CombinationPart part) {
            var vm = new CombinationPartEditViewModel {
                Part = part,
                CombinationContainer = part.CombinationContainerPart
            };

            var allAttributes = GetAllAttributes(vm.Part);

            vm.AllAttributes = allAttributes.Select(a =>
                new CombinationAttribute {
                    AttributeId = a.Id,
                    AttributeDisplayName = string.IsNullOrWhiteSpace(a.DisplayName) ? _contentManager.GetItemMetadata(a).DisplayText : a.DisplayName,
                    Values = a.AttributeValues.Select(v => new AttributeValue { Id = v.Id, Text = v.Text }).ToList(),
                    SelectedAttributeValue = -1
                }).ToList();
            return vm;
        }

        private IEnumerable<ProductAttributePart> GetAllAttributes(CombinationPart part) {
            // get list of attributes we'll be able to use for combinations
            var allAttributes = _productAttributeAdminServices
                .GetAllProductAttributeParts();

            // if the localization part is present, we select the attributes by culture
            var culture = _workContextAccessor.GetContext().CurrentCulture;
            if (part.ContentItem.As<LocalizationPart>() != null && part.ContentItem.As<LocalizationPart>().Culture != null) {
                culture = part.ContentItem.As<LocalizationPart>().Culture.Culture;
            }
            allAttributes = allAttributes
                .Where(a => _localizationService.GetContentCulture(a.ContentItem) == culture);

            return allAttributes;
        }
    }
}
