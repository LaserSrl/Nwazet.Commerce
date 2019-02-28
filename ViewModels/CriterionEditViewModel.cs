using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    public class CriterionEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public ApplicabilityCriterionDescriptor Criterion { get; set; }
        public dynamic Form { get; set; }
    }
}
