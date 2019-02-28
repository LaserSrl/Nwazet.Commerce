using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingManager : IFlexibleShippingManager {
        private readonly IEnumerable<IApplicabilityCriterionProvider> _criterionProviders;

        public FlexibleShippingManager(
            IEnumerable<IApplicabilityCriterionProvider> criterionProviders) {

            _criterionProviders = criterionProviders;
        }

        public IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> DescribeCriteria() {
            var context = new DescribeCriterionContext();

            foreach (var provider in _criterionProviders) {
                provider.Describe(context);
            }

            return context.Describe();
        }
    }
}
