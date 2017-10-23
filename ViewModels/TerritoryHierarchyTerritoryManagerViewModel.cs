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
    public class TerritoryHierarchyTerritoryManagerViewModel {
        public TerritoryHierarchyPart Part { get; set; }
        public ContentItem ContentItem { get; set; }

        public int TerritoriesCount { get; set; }

        public int FirstLevelCount { get; set; }

        public TerritoryHierarchyTerritoryManagerViewModel(
            TerritoryHierarchyPart part) {

            Part = part;
            ContentItem = part.ContentItem;

            TerritoriesCount = part.Territories.Count();
        }
    }
}
