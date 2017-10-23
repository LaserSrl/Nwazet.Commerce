using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyTerritoriesViewModel {

        public TerritoryHierarchyPart HierarchyPart { get; set; }
    }
}
