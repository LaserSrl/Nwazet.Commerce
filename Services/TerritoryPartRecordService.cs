using Nwazet.Commerce.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Territories")]
    public class TerritoryPartRecordService : ITerritoryPartRecordService {
        private readonly IRepository<TerritoryPartRecord> _territoryPartRecord;

        public TerritoryPartRecordService(
            IRepository<TerritoryPartRecord> territoryPartRecord) {
            _territoryPartRecord = territoryPartRecord;
        }

        // TODO : Join CommonVersionRecord? 
        public List<TerritoryPartRecord> GetHierarchyTerritories(TerritoryHierarchyPart hierarchy) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.Hierarchy.Id == hierarchy.Record.Id)
                .ToList();
        }

        public int GetHierarchyTerritoriesCount(TerritoryHierarchyPart hierarchy) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.Hierarchy.Id == hierarchy.Record.Id)
                .Count();
        }

        // TODO : Join CommonVersionRecord? 
        public List<TerritoryPartRecord> GetTerritoriesChild(TerritoryPart territory) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.ParentTerritory.Id == territory.Record.Id)
                .ToList();
        }

        public int GetTerritoriesChildCount(TerritoryPart territory) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.ParentTerritory.Id == territory.Record.Id)
                .Count();
        }

    }
}
