using Nwazet.Commerce.ApplicabilityCriteria;
using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingManager : IFlexibleShippingManager {
        private readonly IEnumerable<IApplicabilityCriterionProvider> _criterionProviders;
        private readonly IContentManager _contentManager;
        private readonly ITokenizer _tokenizer;
        private readonly IRepository<ApplicabilityCriterionRecord> _criteriaRepository;

        public FlexibleShippingManager(
            IEnumerable<IApplicabilityCriterionProvider> criterionProviders,
            IContentManager contentManager,
            ITokenizer tokenizer,
            IRepository<ApplicabilityCriterionRecord> criteriaRepository) {

            _criterionProviders = criterionProviders;
            _contentManager = contentManager;
            _tokenizer = tokenizer;
            _criteriaRepository = criteriaRepository;
        }

        public IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> DescribeCriteria() {
            var context = new DescribeCriterionContext();

            foreach (var provider in _criterionProviders) {
                provider.Describe(context);
            }

            return context.Describe();
        }

        public ApplicabilityCriterionDescriptor GetCriteria(
            string category, string type) {

            return DescribeCriteria()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(c =>
                    c.Category == category
                    && c.Type == type
                );
        }

        public bool TestCriteria(int methodId, ApplicabilityContext context) {
            // get the method
            var methodPart = _contentManager.Get<FlexibleShippingMethodPart>(methodId);
            return TestCriteria(methodPart, context);
        }

        private bool TestCriteria(FlexibleShippingMethodPart methodPart, ApplicabilityContext context) {
            var testResult = methodPart != null;
            if (testResult) {
                // prepare tokens
                Dictionary<string, object> tokens = new Dictionary<string, object>();
                tokens.Add("Content", methodPart.ContentItem);
                // iterate over criteria
                foreach (var criterion in methodPart.ApplicabilityCriteria) {
                    var tokenizedState = _tokenizer.Replace(criterion.State, tokens);
                    var criterionContext = new CriterionContext {
                        IsApplicable = testResult,
                        ApplicabilityContext = context,
                        State = FormParametersHelper.ToDynamic(tokenizedState),
                        FlexibleShippingMethodRecord = methodPart.Record
                    };

                    var descriptor = GetCriteria(criterion.Category, criterion.Type);
                    // descriptor should exist
                    if (descriptor == null) {
                        continue;
                    }

                    descriptor.TestCriterion(criterionContext);
                    testResult = criterionContext.IsApplicable;
                }
            }
            return testResult;
        }

        public void DeleteAllCriteria(int methodId) {
            // get the method
            var methodPart = _contentManager.Get<FlexibleShippingMethodPart>(methodId);
            DeleteAllCriteria(methodPart);
        }

        private void DeleteAllCriteria(FlexibleShippingMethodPart methodPart) {
            if (methodPart != null) {
                foreach (var criterion in methodPart.ApplicabilityCriteria) {
                    DeleteCriterion(criterion);
                }
            }
        }

        public void DeleteCriterion(int criterionId) {
            var record = _criteriaRepository.Get(criterionId);
            if (record != null) {
                DeleteCriterion(record);
            }
        }

        private void DeleteCriterion(ApplicabilityCriterionRecord record) {
            record.FlexibleShippingMethodRecord.ApplicabilityCriteria.Remove(record);
            _criteriaRepository.Delete(record);
        }
    }
}
