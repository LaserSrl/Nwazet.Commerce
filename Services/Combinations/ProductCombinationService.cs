using Nwazet.Commerce.Models;
using Nwazet.Commerce.Settings.Combinations;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
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

        public ProductCombinationService(
            IContentManager contentManager,
            IRepository<ProductAttributePartRecord> productAttributesRepository,
            Lazy<IEnumerable<IContentHandler>> handlers) {

            _contentManager = contentManager;
            _productAttributesRepository = productAttributesRepository;
            _handlers = handlers;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<IContentHandler> Handlers {
            get { return _handlers.Value; }
        }

        public IEnumerable<CombinationPart> CreateCombinations(
            CombinationContainerPart containerPart,
            IEnumerable<AttributesToCombine> attributesToCombines) {
            // TODO: parameter validation
            var partSettings = containerPart.TypePartDefinition
                .Settings.GetModel<CombinationContainerPartSettings>();

            var contentType = partSettings?.CombinationTypeName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(contentType)) {
                return Enumerable.Empty<CombinationPart>();
            }

            // Based on their Ids, get the attributes
            var attributeIds = attributesToCombines
                    .Select(atc => atc.AttributeId)
                    .Distinct();
            var attributeParts = _contentManager
                .GetMany<ProductAttributePart>(attributeIds, VersionOptions.Latest, QueryHints.Empty);
            var attributePartRecords = _productAttributesRepository
                .Table
                .Where(papr => attributeIds
                    .Contains(papr.Id))
                .ToList();

            // Create the contents
            var createdItems = new List<CombinationPart>();
            foreach (var atc in attributesToCombines) {
                var newItem = _contentManager.New(contentType);
                // We don't publish the item immediately: users that are creating the 
                // combinations will have the option to do that themselves.
                _contentManager.Create(newItem, VersionOptions.Draft);
                // Set the values on the CombinationPartRecord
                var combinationPart = newItem.As<CombinationPart>();
                // Simulate an update
                var context = new UpdateContentContext(newItem);
                Handlers.Invoke(handler => handler.Updating(context), Logger);
                // Here the transaction that's creating the contents hasn't been 
                // committed yet, so we can't interact directly with combinationPart.Record
                combinationPart.CombinationContainerPartField.Value = containerPart;
                combinationPart.ProductAttributePartField.Value = attributeParts.FirstOrDefault(p =>p.Id == atc.AttributeId); //TODO
                combinationPart.ProductAttributeValue = atc.AttributeValue; //TODO
                Handlers.Invoke(handler => handler.Updated(context), Logger);

                //var combinationRecord = combinationPart.Record;
                //combinationRecord.CombinationContainerPartRecord = containerPart.Record;
                //// TODO: use a data structure to avoid doing FirstOrDefault every time
                //combinationRecord.ProductAttributePartRecord = attributePartRecords
                //    .FirstOrDefault(papc => papc.Id == atc.AttributeId);
                //combinationRecord.ProductAttributeValue = atc.AttributeValue;

                createdItems.Add(combinationPart);

                // Sync information from the container to the combination
                // TODO: providers

            }

            return createdItems;
        }
    }
}
