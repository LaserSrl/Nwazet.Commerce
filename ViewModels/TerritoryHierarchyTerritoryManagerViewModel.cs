using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyTerritoryManagerViewModel {
        public TerritoryHierarchyPart Part { get; set; }
        public ContentItem ContentItem { get; set; }

        public int TerritoriesCount { get; set; }

        public int TopLevelCount { get; set; }

        public TerritoryHierarchyTerritoryManagerViewModel(
            TerritoryHierarchyPart part, ITerritoryPartRecordService _territoryPartRecordService) {

            Part = part;
            ContentItem = part.ContentItem;

            TerritoriesCount = _territoryPartRecordService.GetHierarchyTerritoriesCount(part); //part.Record.Territories?.Count() ?? 0;
        }
    }
}
