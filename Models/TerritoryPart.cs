using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryPart : ContentPart<TerritoryPartRecord> {

        public static string PartName = "TerritoryPart";
    }
}
