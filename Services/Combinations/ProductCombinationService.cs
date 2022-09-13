using Nwazet.Commerce.Models;
using Nwazet.Commerce.Settings.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ProductCombinationService : IProductCombinationService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<ProductAttributePartRecord> _productAttributesRepository;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IProductAttributeAdminServices _productAttributeAdminServices;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Lazy<IEnumerable<ICombinationDetailProvider>> _combinationDetailProviders;

        public ProductCombinationService(
            IContentManager contentManager,
            IRepository<ProductAttributePartRecord> productAttributesRepository,
            Lazy<IEnumerable<IContentHandler>> handlers,
            IProductAttributeAdminServices productAttributeAdminServices,
            IContentDefinitionManager contentDefinitionManager,
            Lazy<IEnumerable<ICombinationDetailProvider>> combinationDetailProviders) {

            _contentManager = contentManager;
            _productAttributesRepository = productAttributesRepository;
            _handlers = handlers;
            _productAttributeAdminServices = productAttributeAdminServices;
            _contentDefinitionManager = contentDefinitionManager;
            _combinationDetailProviders = combinationDetailProviders;

            T = NullLocalizer.Instance;

            Logger = NullLogger.Instance;

            _attributeNames = new Dictionary<int, string>();
            _attributeValueText = new Dictionary<int, string>();
        }

        public ILogger Logger { get; set; }

        public Localizer T { get; set; }
        
        private IEnumerable<IContentHandler> Handlers {
            get { return _handlers.Value; }
        }

        private IEnumerable<ICombinationDetailProvider> DetailProviders {
            // We don't set up an infrastructure like the IContentHandler.Invoke
            // to safely use these providers because it's probably enough to just do
            // it as a method here because these providers currently aren't used
            // elsewhere.
            get { return _combinationDetailProviders.Value; }
        }

        public string GetCombinationContentType(
            CombinationContainerPart containerPart) {
            var partSettings = containerPart.TypePartDefinition
                .Settings.GetModel<CombinationContainerPartSettings>();

            return partSettings?.CombinationTypeName ?? string.Empty;
        }

        public CombinationPart GetDummyCombination(CombinationContainerPart container) {
            if (container == null) {
                return null;
            }
            // In most cases I could actually use any of the combinations that may
            // have already been created for container. However:
            // 1- Something may edit that combination down the line, even if that is a mistake.
            // 2- The ContentType for combinations may have been changed for container.
            // TODO: should we do something special for 2?
            var ct = GetCombinationContentType(container);
            return _contentManager.New<CombinationPart>(ct);
        }

        public IEnumerable<CombinationPart> CreateCombinations(
            CombinationContainerPart containerPart,
            IEnumerable<IEnumerable<AttributesToCombine>> attributesCombinations) {

            if (containerPart == null) {
                throw new ArgumentNullException("containerPart");
            }
            if (attributesCombinations == null) {
                throw new ArgumentNullException("attributesCombinations");
            }

            var contentType = GetCombinationContentType(containerPart);
            if (string.IsNullOrWhiteSpace(contentType)) {
                return Enumerable.Empty<CombinationPart>();
            }

            var combinations = attributesCombinations.Where(i => i != null && i.Any());
            if (!combinations.Any()) {
                return Enumerable.Empty<CombinationPart>();
            }

            // Create the contents
            var createdItems = new List<CombinationPart>();
            foreach (var combination in combinations) {
                var newItem = _contentManager.New(contentType);
                // We don't publish the item immediately: users that are creating the 
                // combinations will have the option to do that themselves.
                _contentManager.Create(newItem, VersionOptions.Draft);
                // "Simulate" an update as if the user has created a new ContentItem
                var context = new UpdateContentContext(newItem);
                Handlers.Invoke(handler => handler.Updating(context), Logger);
                // Set the values on the CombinationPartRecord
                var combinationPart = newItem.As<CombinationPart>();
                combinationPart.CombinationContainerPartField.Value = containerPart;
                combinationPart.ProductAttributeValues = combination;
                SaveAttributes(combinationPart, combination);
                // Sync information from the container to the combination
                foreach (var provider in DetailProviders) {
                    provider.Synchronize(containerPart, combinationPart);
                }
                Handlers.Invoke(handler => handler.Updated(context), Logger);
                // We are not able to properly validate all the information passed to
                // the new ContentItem created, because that would entail invoking
                // all drivers related to it. For this reason we are not publishing
                // the ContentItem: it will be the user's responsibility to do that.
                createdItems.Add(combinationPart);
            }

            return createdItems;
        }

        public void SaveAttributes(CombinationPart combinationPart, IEnumerable<AttributesToCombine> attributes) {
            //var paps = _contentManager.Query<ProductAttributePart>(VersionOptions.Latest)
            //    .ForContentItems(attributes.Select(a => a.AttributeValue))
            //    .List();

            //combinationPart.ProductAttributeParts = paps;
        }

        private Dictionary<int, string> _attributeNames;
        private Dictionary<int, string> _attributeValueText;
        public string CombinationDisplayText(CombinationPart combinationPart) {
            if (combinationPart == null) {
                throw new ArgumentNullException("combinationPart");
            }
            // TODO: Use AdminFilter to prepare a different text for backoffice vs frontend
            var comboValues = combinationPart.ProductAttributeValues;
            if (comboValues == null) {
                return T("Undefined").Text;
            }

            // get the attributes, because we need the title/displayname
            // Some are memorized:
            var attributeIdsToFetch = comboValues == null ? new List<int>() : comboValues
                .Select(cv => cv.AttributeId)
                .Except(_attributeNames.Keys);
            // fetch
            var attributes = _productAttributeAdminServices
                .GetProductAttributeParts(attributeIdsToFetch.ToArray());
            // memorize
            foreach (var newAttribute in attributes) {
                var attDisplayText = _contentManager.GetItemMetadata(newAttribute).DisplayText;
                // TODO: fallbacks for the displaytext
                _attributeNames.Add(newAttribute.Id, attDisplayText);
                // Memorize the text for the attribute values
                foreach (var attVal in newAttribute.AttributeValues) {
                    if (!_attributeValueText.ContainsKey(attVal.Id)) {
                        _attributeValueText.Add(attVal.Id, attVal.Text);
                    }
                }
            }
            var textElements = comboValues == null ? new List<string>() : comboValues
                .Select(cv => GetDisplayText(cv)).Where(s => !string.IsNullOrWhiteSpace(s));
            // TODO: when Attributes get their own records for values, handle them here properly
            return string.Join(", ", textElements);
        }

        private string GetDisplayText(AttributesToCombine cv) {
            if (_attributeNames.ContainsKey(cv.AttributeId) && _attributeValueText.ContainsKey(cv.AttributeValue)) {
                return _attributeNames[cv.AttributeId] + " " + _attributeValueText[cv.AttributeValue];
            } else {
                return T("Invalid attribute").Text;
            }
        }

        public void CreateCombinationType(
            string containerTypeName, string combinationTypeName) {

            Func<ContentTypeDefinitionBuilder, ContentTypeDefinitionBuilder> comboDefinition = 
                cfg => cfg
                    .WithPart("CombinationPart")
                    .WithPart("CommonPart")
                    .WithIdentity()
                    // Make the new ContentType Draftable, so when we create new combinations they
                    // are not automatically published.
                    .Draftable()
                    // To make sure that Products can't be created directly from the "normal" backend 
                    // controller for ContentItems we need it Listable and Createable properties to
                    // both be false.
                    .Creatable(false).Listable(false)
                    // TODO: handle securable / permissions for the combinations
                    ;

            _contentDefinitionManager.AlterTypeDefinition(combinationTypeName,
                cfg => {
                    cfg = comboDefinition(cfg);
                    // Have a service add other parts that are related to product
                    // so we can synchronize values when creating combinations
                    foreach (var provider in DetailProviders) {
                        cfg = provider.AlterCombinationDefinition(cfg, containerTypeName);
                    }
                });
        }

        public IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part,
            dynamic shapeHelper) {

            var result = new List<CombinationDetailShape>();
            foreach (var provider in DetailProviders) {
                result.AddRange(provider.GetCombinationDetailShapes(part, shapeHelper));
            }
            return result;
        }
    }
}
