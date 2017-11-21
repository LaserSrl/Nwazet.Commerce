using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryPart : ContentPart<TerritoryPartRecord> {

        public static string PartName = "TerritoryPart";

        private readonly LazyField<IEnumerable<ContentItem>> _children =
            new LazyField<IEnumerable<ContentItem>>();

        public LazyField<IEnumerable<ContentItem>> ChildrenField {
            get { return _children; }
        }

        // This contains the direct children of this territory
        public IEnumerable<ContentItem> Children {
            get { return _children.Value; }
            // no setter, because this is "filled" thanks to a 1-to-n relationship to TerritoryPartRecords
        }
        
        private readonly LazyField<ContentItem> _hierarchy =
            new LazyField<ContentItem>();

        public LazyField<ContentItem> HierarchyField {
            get { return _hierarchy; }
        }

        public ContentItem Hierarchy {
            get { return _hierarchy.Value; }
        }

        public TerritoryHierarchyPart HierarchyPart {
            get { return Hierarchy?.As<TerritoryHierarchyPart>(); }
        }

        private readonly LazyField<ContentItem> _parent =
            new LazyField<ContentItem>();

        public LazyField<ContentItem> ParentField {
            get { return _parent; }
        }

        public ContentItem Parent {
            get { return _parent.Value; }
        }

        public TerritoryPart ParentPart {
            get { return Parent?.As<TerritoryPart>(); }
        }

        private readonly LazyField<int> _allChildrenCount =
            new LazyField<int>();

        public LazyField<int> AllChildrenCountField {
            get { return _allChildrenCount; }
        }

        public int AllChildrenCount {
            get { return _allChildrenCount.Value; }
        }
    }
}
