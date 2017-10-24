using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
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
        public ContentItem HierarchyItem { get; set; }

        // First level of the tree of territories in this hierarchy
        // It may contain further levels, as each node's Nodes property gets populated.
        public IList<TerritoryHierarchyTreeNode> Nodes { get; set; }
    }
}
