using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPart : ContentPart<TerritoryHierarchyPartRecord> {

        public static string PartName = "TerritoryHierarchyPart";
        
        private readonly LazyField<IEnumerable<ContentItem>> _topLevel =
            new LazyField<IEnumerable<ContentItem>>();

        public LazyField<IEnumerable<ContentItem>> TopLevelField {
            get { return _topLevel; }
        }
        public IEnumerable<ContentItem> TopLevel {
            get {
                return _topLevel.Value;
            }
        }

        public string TerritoryType {
            get { return Retrieve(r => r.TerritoryType); }
            set { Store(r => r.TerritoryType, value); }
        }
    }
}
