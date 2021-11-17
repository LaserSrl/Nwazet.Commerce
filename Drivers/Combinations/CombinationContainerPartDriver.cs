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
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationContainerPartDriver : ContentPartDriver<CombinationContainerPart>,
        // We to implement this interface to interact with ProductPartDriver and ShoppingCartController
        IProductAttributesDriver {

        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IContentManager _contentManager;
        private readonly IProductCombinationService _productCombinationService;
        private readonly IAuthorizer _authorizer;

        public CombinationContainerPartDriver(
            IProductAttributeAdminServices productAttributeAdminServices,
            IContentManager contentManager,
            IProductCombinationService productCombinationService,
            IAuthorizer authorizer) {

            _productAttributeAdminServices = productAttributeAdminServices;
            _contentManager = contentManager;
            _productCombinationService = productCombinationService;
            _authorizer = authorizer;

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
                if (vm.Part.Id == 0) {
                    newFactory = () => {
                        return shapeHelper.EditorTemplate(
                            TemplateName: "Parts/Combinations/CombinationContainerPart.New",
                            Model: vm,
                            Prefix: Prefix
                            );
                    };
                } else {
                    editorFactory = () => {
                        // get list of attributes we'll be able to use for combinations
                        var allAttributes = _productAttributeAdminServices
                                .GetAllProductAttributeParts();
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
                    _productCombinationService.AdminDisplayText(combo));
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
            // Dictionary of detail information for each combination, that we can use
            // to dynamically update the UI. The key is the Id of the combination. The
            // value is the collection of detail elements.
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
                CombinationParts: publishedCombinationParts,
                CombinationDetails: combinationDetails
                );
        }

        public bool ValidateAttributes(IContent product, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues) {
            // We aren't really going to do anything about attributes here
            return true;
            // TODO: how about the case where combinations have their own attributes?
            // We aren't displaying the Combinations as products, so those should probably
            // be either handled here, or prevented somehow.
        }
    }
}
