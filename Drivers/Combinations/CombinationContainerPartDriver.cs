using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Combinations;
using Nwazet.Commerce.Settings.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using CorePermissions = Orchard.Core.Contents.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartDriver : ContentPartCloningDriver<CombinationContainerPart>,
        // We to implement this interface to interact with ProductPartDriver and ShoppingCartController
        IProductAttributesDriver {

        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IContentManager _contentManager;
        private readonly IProductCombinationService _productCombinationService;
        private readonly IAuthorizer _authorizer;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public CombinationContainerPartDriver(
            IProductAttributeAdminServices productAttributeAdminServices,
            IContentManager contentManager,
            IProductCombinationService productCombinationService,
            IAuthorizer authorizer,
            IProductService productService,
            ILocalizationService localizationService,
            IWorkContextAccessor workContextAccessor) {

            _productAttributeAdminServices = productAttributeAdminServices;
            _contentManager = contentManager;
            _productCombinationService = productCombinationService;
            _authorizer = authorizer;
            _productService = productService;
            _localizationService = localizationService;
            _workContextAccessor = workContextAccessor;

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
            var shapes = new List<DriverResult>();
            var dummyCombination = _productCombinationService.GetDummyCombination(vm.Part);
            Func<dynamic> unauthorizedFactory = () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/CombinationContainerPart.Empty");
            Func<dynamic> newFactory = () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/CombinationContainerPart.Empty");
            Func<dynamic> editorFactory = () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Combinations/CombinationContainerPart.Empty");
            if (!_authorizer.Authorize(CorePermissions.CreateContent, dummyCombination)) {
                unauthorizedFactory = () => {
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Combinations/CombinationContainerPart.Unauthorized",
                        Model: vm,
                        Prefix: Prefix
                        );
                };
            } else {
                // check if you are creating
                if (vm.Part.Id == 0) {
                    newFactory = () => {
                        return shapeHelper.EditorTemplate(
                            TemplateName: "Parts/Combinations/CombinationContainerPart.New",
                            Model: vm,
                            Prefix: Prefix
                            );
                    };
                }
                // check if the content item has a translation associated with it
                else if (vm.Part.ContentItem.As<LocalizationPart>() != null && vm.Part.ContentItem.As<LocalizationPart>().Culture == null) {
                    // if the container that doesn't have the translation has combinations I show it
                    // the status will show "missing culture"
                    if (vm.CurrentCombinations.Any()) {
                        editorFactory = () => {
                            var allAttributes = _productAttributeAdminServices
                                .GetAllProductAttributeParts();
                            // get list of attributes we'll be able to use for combinations
                            vm.AllAttributeParts = allAttributes;

                            return shapeHelper.EditorTemplate(
                                TemplateName: "Parts/Combinations/CombinationContainerPart",
                                Model: vm,
                                Prefix: Prefix
                                );
                        };
                    } else {
                        newFactory = () => {
                            return shapeHelper.EditorTemplate(
                                TemplateName: "Parts/Combinations/CombinationContainerPart.New",
                                Model: vm,
                                Prefix: Prefix
                                );
                        };
                    }
                } else {
                    editorFactory = () => {
                        // get list of attributes we'll be able to use for combinations
                        var allAttributes = _productAttributeAdminServices
                                .GetAllProductAttributeParts();

                        var culture = _workContextAccessor.GetContext().CurrentCulture;
                        if (vm.Part.ContentItem.As<LocalizationPart>() != null &&
                            vm.Part.ContentItem.As<LocalizationPart>().Culture != null) {
                            culture = vm.Part.ContentItem.As<LocalizationPart>().Culture.Culture;
                        }
                        allAttributes = allAttributes
                               .Where(a => _localizationService.GetContentCulture(a.ContentItem) == culture);

                        vm.AllAttributeParts = allAttributes;

                        return shapeHelper.EditorTemplate(
                            TemplateName: "Parts/Combinations/CombinationContainerPart",
                            Model: vm,
                            Prefix: Prefix
                            );
                    };
                }
            }
            shapes.Add(ContentShape("Parts_CombinationContainerPart_Editor_Unauthorized", unauthorizedFactory));
            shapes.Add(ContentShape("Parts_CombinationContainerPart_Editor_New", newFactory));
            shapes.Add(ContentShape("Parts_CombinationContainerPart_Editor", editorFactory));
            return Combined(shapes.ToArray());
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
                    _productCombinationService.CombinationDisplayText(combo));
            }
            return new CombinationContainerPartEditViewModel() {
                Part = part,
                CurrentCombinations = combinationContents,
                CombinationTitles = comboTitles,
                CombinationTypeName = partSettings?.CombinationTypeName ?? string.Empty
            };
        }

        public dynamic GetAttributeDisplayShape(IContent product, dynamic shapeHelper) {
            var combinationContainerPart = product.As<CombinationContainerPart>();
            if (combinationContainerPart == null) {
                return null;
            }
            // Only get the published CombinationParts
            var allCombinationParts = combinationContainerPart.CombinationParts;
            var publishedCombinationParts = _contentManager
                .GetMany<CombinationPart>(
                    allCombinationParts.Select(cp => cp.Id),
                    VersionOptions.Published,
                    QueryHints.Empty)
                .ToList();
            // Handle availability of the products for those CombinationParts
            var availableCombinationParts = publishedCombinationParts
                .Where(cp => MayAddToCart(cp));
            var unavailableCombinationParts = publishedCombinationParts
                .Where(cp => !MayAddToCart(cp));
            // Dictionary of detail information for each combination, that we can use
            // to dynamically update the UI. The key is the Id of the combination. The
            // value is the collection of detail elements.
            // TODO: should we prep this only for available combinations?
            var combinationDetails = new Dictionary<int, IEnumerable<CombinationDetailShape>>();
            foreach (var combo in publishedCombinationParts) {
                combinationDetails.Add(
                    combo.Id,
                    _productCombinationService.GetCombinationDetailShapes(combo, shapeHelper)
                    );
            }
            // Based on those combinations we'll have to display a shape with
            // options for the user to choose, as if the attributes used to generate
            // the combinations were just attributes. The shape will also include a
            // js library to handle updating informations on screen, such as the Id
            // of the product that will be actually added to the cart, its availability,
            // its price and so on.
            return shapeHelper.Parts_CombinationContainer(
                ContentItem: product,
                AllCombinationParts: publishedCombinationParts,
                AvailableCombinationParts: availableCombinationParts,
                UnavailableCombinationParts: unavailableCombinationParts,
                CombinationDetails: combinationDetails
                );
        }

        public bool ValidateAttributes(
            IContent product, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues) {
            // We aren't really going to do anything about attributes here
            return true;
            // TODO: how about the case where combinations have their own attributes?
            // We aren't displaying the Combinations as products, so those should probably
            // be either handled here, or prevented somehow.
        }

        private bool MayAddToCart(CombinationPart combination) {
            var productPart = combination.As<ProductPart>();
            if (productPart == null) {
                // TODO: is this right?
                // Combinations without a ProductPart shouldn't be added to the cart anyway, 
                // because they can't work as "independent" products.
                return false;
            }
            return _productService.MayAddToCart(productPart);
        }

        //// TODO Import/Export
        //protected override void Importing(CombinationContainerPart part, ImportContentContext context) {
        //    if (context.Data.Element(part.PartDefinition.Name) == null) {
        //        return;
        //    }

        //    var CombinationParts = context.Attribute(part.PartDefinition.Name, "CombinationParts");
        //    foreach (var comb in CombinationParts.Split(';')) {
        //        if (!string.IsNullOrEmpty(comb)) {
        //            part.CombinationParts.Add(context.GetItemFromSession(comb).As<CombinationPart>());
        //        }
        //    }
        //}

        //protected override void Exporting(CombinationContainerPart part, ExportContentContext context) {
        //    var root = context.Element(part.PartDefinition.Name);

        //    var combinationParts = "";
        //    foreach (var combinationPart in part.CombinationParts) {
        //        combinationParts+=_contentManager.GetItemMetadata(combinationPart).Identity+";";
        //    }
        //    root.SetAttributeValue("CombinationParts", combinationParts);
        //}

        //protected override void Cloning(CombinationContainerPart originalPart, CombinationContainerPart clonePart, CloneContentContext context) {
        //    clonePart.Record.Id = originalPart.Record.Id;
        //}
    }
}
