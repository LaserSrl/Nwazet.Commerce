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

            Logger = NullLogger.Instance;

            _attributeNames = new Dictionary<int, string>();
        }

        public ILogger Logger { get; set; }

        private IEnumerable<IContentHandler> Handlers {
            get { return _handlers.Value; }
        }

        private IEnumerable<ICombinationDetailProvider> DetailProviders {
            get { return _combinationDetailProviders.Value; }
        }

        public IEnumerable<CombinationPart> CreateCombinations(
            CombinationContainerPart containerPart,
            IEnumerable<IEnumerable<AttributesToCombine>> attributesCombinations) {
            // TODO: parameter validation
            var partSettings = containerPart.TypePartDefinition
                .Settings.GetModel<CombinationContainerPartSettings>();

            var contentType = partSettings?.CombinationTypeName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(contentType)) {
                return Enumerable.Empty<CombinationPart>();
            }

            // Create the contents
            var createdItems = new List<CombinationPart>();
            foreach (var combination in attributesCombinations) {
                var newItem = _contentManager.New(contentType);
                // We don't publish the item immediately: users that are creating the 
                // combinations will have the option to do that themselves.
                _contentManager.Create(newItem, VersionOptions.Draft);
                // Simulate an update
                var context = new UpdateContentContext(newItem);
                Handlers.Invoke(handler => handler.Updating(context), Logger);
                // Here the transaction that's creating the contents hasn't been 
                // committed yet, so we can't interact directly with combinationPart.Record
                Handlers.Invoke(handler => handler.Updated(context), Logger);

                // Set the values on the CombinationPartRecord
                var combinationPart = newItem.As<CombinationPart>();
                combinationPart.CombinationContainerPartField.Value = containerPart;
                combinationPart.ProductAttributeValues = combination;

                // Sync information from the container to the combination
                // TODO: providers

                createdItems.Add(combinationPart);
            }

            return createdItems;
        }

        private Dictionary<int, string> _attributeNames;
        public string AdminDisplayText(CombinationPart cPart) {
            var comboValues = cPart.ProductAttributeValues;
            // get the attributes, because we need the title/displayname
            // Some are memorized:
            var attributeIdsToFetch = comboValues
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
            }
            var textElements = comboValues
                .Select(cv => _attributeNames[cv.AttributeId] + " " + cv.AttributeValue);
            // TODO: when Attributes get their own records for values, handle them here properly
            return string.Join(", ", textElements);
        }

        public void CreateCombinationType(
            string containerTypeName, string combinationTypeName) {

            Func<ContentTypeDefinitionBuilder, ContentTypeDefinitionBuilder> comboDefinition = 
                cfg => cfg
                    .WithPart("CombinationPart")
                    .WithPart("CommonPart")
                    .WithIdentity();
            comboDefinition = cfg => comboDefinition(cfg).Draftable();
            // TODO: handle securable / permissions for the combinations
            // TODO: make sure that Products can't be created directly from 
            // "normal" backend controllers
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
