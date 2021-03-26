using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyTerritoriesViewModel {
        public TerritoryHierarchyTerritoriesViewModel() {
            Nodes = new List<TerritoryHierarchyTreeNode>();
            ProgressiveIndex = 0;
        }

        public TerritoryHierarchyPart HierarchyPart { get; set; }
        public ContentItem HierarchyItem { get; set; }

        public IList<TerritoryHierarchyTreeNode> Nodes { get; set; }

        public bool CanAddMoreTerritories { get; set; }

        public int TerritoryNodeId { get; set; }

        public int ProgressiveIndex { get; set; }

        public string UpdatedNodesIds { get; set; }
    }
}
