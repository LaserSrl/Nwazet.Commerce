using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryPartRecord : ContentPartRecord {

        public virtual TerritoryInternalRecord TerritoryInternalRecord { get; set; }

        public virtual TerritoryPartRecord ParentTerritory { get; set; }
    }
}
