using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.ApplicabilityCriteria;
using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Tokens;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingManager : IFlexibleShippingManager {
        private readonly IEnumerable<IApplicabilityCriterionProvider> _criterionProviders;
        private readonly IContentManager _contentManager;
        private readonly ITokenizer _tokenizer;

        public FlexibleShippingManager(
            IEnumerable<IApplicabilityCriterionProvider> criterionProviders,
            IContentManager contentManager,
            ITokenizer tokenizer) {

            _criterionProviders = criterionProviders;
            _contentManager = contentManager;
            _tokenizer = tokenizer;
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
            var testResult = true;
            // get the method
            var methodPart = _contentManager.Get<FlexibleShippingMethodPart>(methodId);
            testResult = methodPart != null;
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
    }
}
