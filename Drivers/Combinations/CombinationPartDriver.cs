using Newtonsoft.Json;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPartDriver : ContentPartCloningDriver<CombinationPart> {
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IProductCombinationService _productCombinationService;
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<ProductAttributeValueRecord> _attributeValueRepo;
        private readonly INotifier _notifier;

        public CombinationPartDriver(
            IOrchardServices orchardServices,
            IProductAttributeAdminServices productAttributeAdminServices,
            IProductCombinationService productCombinationService,
            ILocalizationService localizationService,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IRepository<ProductAttributeValueRecord> attributeValueRepo,
            INotifier notifier) {

            Services = orchardServices;
            _productAttributeAdminServices = productAttributeAdminServices;
            _productCombinationService = productCombinationService;
            _localizationService = localizationService;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _attributeValueRepo = attributeValueRepo;
            _notifier = notifier;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T;
        public IOrchardServices Services { get; private set; }
        public ILogger Logger;

        protected override string Prefix {
            get { return "CombinationPart"; }
        }

        protected override DriverResult Display(CombinationPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(CombinationPart part, dynamic shapeHelper) {
            var vm = CreateVM(part);
            return EditorShape(vm, shapeHelper);
        }

        protected override DriverResult Editor(CombinationPart part, IUpdateModel updater, dynamic shapeHelper) {
            // CombinationConfigurationAdminController.cs Create(), Creates the content of the element
            // in the driver takes care of assigning the created combination
            var vm = CreateVM(part);

            updater.TryUpdateModel(vm, Prefix, null, null);

            // the driver does not have to do anything 
            // if there are already assigned combinations
            if (vm.Part.ProductAttributeValues != null && part.ProductAttributeValues.Any()) {
                return EditorShape(vm, shapeHelper);
            }

            // after tyyupdatemodel missing the values of attributes
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
            _productCombinationService.SaveAttributes(part, attributeToCombine);

            Services.Notifier.Information(T("A new combination has been created"));

            return EditorShape(vm, shapeHelper);
        }

        private DriverResult EditorShape(CombinationPartEditViewModel vm, dynamic shapeHelper) {
            var shapes = new List<DriverResult>();
            if (vm.Part.ProductAttributeValues == null || !vm.Part.ProductAttributeValues.Any()) {
                // shape showing the attributes
                Func<dynamic> attributesFactory = () => {
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Combinations/AttributesCombinationPart",
                        Model: vm,
                        Prefix: Prefix
                        );
                };
                shapes.Add(ContentShape("Parts_AttributesCombinationPart_Editor", attributesFactory));
            }

            Func<dynamic> combinationFactory = () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/CombinationPart",
                    Model: vm,
                    Prefix: Prefix
                );
            };
            shapes.Add(ContentShape("Parts_CombinationPart_Editor", combinationFactory));

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

        protected override void Importing(CombinationPart part, ImportContentContext context) {
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            var containerId = context.Attribute(part.PartDefinition.Name, "CombinationContainerPart");
            if (string.IsNullOrWhiteSpace(containerId)) {
                return;
            }

            var container = _contentManager.ResolveIdentity(new ContentIdentity(containerId));
            var containerPart = container.As<CombinationContainerPart>();
            if (containerPart != null) {
                part.CombinationContainerPartField.Value = containerPart;

                var exportedAttributes = JsonConvert.DeserializeObject<List<AttributesToCombineExportViewModel>>(context
                    .Attribute(part.PartDefinition.Name, "ProductAttributeValues"));

                var atc = new List<AttributesToCombine>();
                foreach (var a in exportedAttributes) {
                    var attribute = _contentManager.ResolveIdentity(new ContentIdentity(a.AttributeId));

                    if (attribute != null) {
                        var attributeId = attribute.Id;

                        var valueId = 0;
                        var valueRecord = _attributeValueRepo.Table
                            .FirstOrDefault(pavr => pavr.GUIdentifier == a.ValueId);
                        if (valueRecord != null) {
                            valueId = valueRecord.Id;

                            atc.Add(new AttributesToCombine {
                                AttributeId = attributeId,
                                AttributeValue = valueId
                            });
                        } else {
                            // TODO: add a flag to CombinationPart to highlight the anomaly.
                            Logger.Error(T("Attribute value with GUIdentifier {0} is missing and cannot be imported.", a.ValueId).Text);
                            _notifier.Error(T("Attribute value with GUIdentifier {0} is missing and cannot be imported.", a.ValueId));
                        }
                    } else {
                        // TODO: add a flag to CombinationPart to highlight the anomaly.
                        Logger.Error(T("Attribute with identity {0} is missing and cannot be imported.", a.AttributeId).Text);
                        _notifier.Error(T("Attribute with identity {0} is missing and cannot be imported.", a.AttributeId));
                    }
                }
                
                part.ProductAttributeValues = atc;
            }
        }

        protected override void Exporting(CombinationPart part, ExportContentContext context) {
            var exportedAttributes = new List<AttributesToCombineExportViewModel>();

            foreach (var a in part.ProductAttributeValues) {
                var attributeId = _contentManager.GetItemMetadata(_contentManager.Get(a.AttributeId)).Identity.ToString();
                var valueId = string.Empty;
                var valueRecord = _attributeValueRepo.Table
                    .FirstOrDefault(pavr => pavr.Id == a.AttributeValue);
                if (valueRecord != null) {
                    valueId = valueRecord.GUIdentifier;
                } else {
                    valueId = Guid.NewGuid().ToString();
                }

                exportedAttributes.Add(new AttributesToCombineExportViewModel {
                    AttributeId = attributeId,
                    ValueId = valueId
                });
            }

            context.Element(part.PartDefinition.Name)
                .SetAttributeValue("ProductAttributeValues", JsonConvert.SerializeObject(exportedAttributes));

            // Export the container id
            context.Element(part.PartDefinition.Name)
                .SetAttributeValue("CombinationContainerPart", _contentManager.GetItemMetadata(part.CombinationContainerPart).Identity);

        }

        protected override void Cloning(CombinationPart originalPart, CombinationPart clonePart, CloneContentContext context) {
            // clone the combination container part id
            clonePart.CombinationContainerPartField.Value =
                originalPart.CombinationContainerPartField.Value;
        }
    }
}
