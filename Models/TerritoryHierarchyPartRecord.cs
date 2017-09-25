using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPartRecord : ContentPartRecord {

        public TerritoryHierarchyPartRecord() {
            Territories = new List<TerritoryPartRecord>();
        }

        public virtual IList<TerritoryPartRecord> Territories { get; set; }
    }
}
