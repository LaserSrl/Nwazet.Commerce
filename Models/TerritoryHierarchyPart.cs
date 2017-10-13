using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPart : ContentPart<TerritoryHierarchyPartRecord> {

        public static string PartName = "TerritoryHierarchyPart";

        private readonly LazyField<IEnumerable<IContent>> _territories = 
            new LazyField<IEnumerable<IContent>>();

        public LazyField<IEnumerable<IContent>> TerritoriesField {
            get { return _territories; }
        }

        public IEnumerable<IContent> Territories {
            get { return _territories.Value; }
            set { _territories.Value = value; }
        }
    }
}
