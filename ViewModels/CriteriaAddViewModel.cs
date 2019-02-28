using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    public class CriteriaAddViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> Criteria { get; set; }
    }
}
