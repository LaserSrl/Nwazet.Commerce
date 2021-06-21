using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class FilterOptions {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Priority { get; set; }
        public string PriorityFrom { get; set; }
        public string PriorityTo { get; set; }
        public TypePriority SelectedPriority {get;set;}
        public FilterOrderBy OrderBy { get; set; }
        public bool Descending { get; set; }
        public FiterOptionState State { get; set; }
        public FiterOptionValueType ValueType { get; set; }
    }
}

public enum FilterOrderBy {
    Name,
    Code,
    Priority
}

public enum FiterOptionState {
    All,
    Published,
    UnPublished
}

public enum FiterOptionValueType {
    All,
    Percent,
    Amount,
    CartAmount
}

public enum TypePriority {
    Equals,
    FromTo,
    From,
    To
}