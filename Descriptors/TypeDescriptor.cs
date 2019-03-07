using Orchard.Localization;
using System.Collections.Generic;

namespace Nwazet.Commerce.Descriptors {
    // This replicates the design used for queries in Orchard.Projections
    public class TypeDescriptor<T> {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public IEnumerable<T> Descriptors { get; set; }
    }
}
