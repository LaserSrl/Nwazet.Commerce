using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IFlexibleShippingManager : IDependency {

        IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> DescribeCriteria();
    }
}
