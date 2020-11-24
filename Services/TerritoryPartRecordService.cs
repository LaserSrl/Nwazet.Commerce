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
        public IQueryable<TerritoryPartRecord> GetHierarchyTerritories(TerritoryHierarchyPart hierarchy) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.Hierarchy.Id == hierarchy.Record.Id);
        }

        public int GetHierarchyTerritoriesCount(TerritoryHierarchyPart hierarchy) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.Hierarchy.Id == hierarchy.Record.Id)
                .Count();
        }

        // TODO : Join CommonVersionRecord? 
        public IQueryable<TerritoryPartRecord> GetTerritoriesChild(TerritoryPart territory) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.ParentTerritory.Id == territory.Record.Id);
        }

        public IQueryable<TerritoryPartRecord> GetTerritoriesChild(TerritoryPartRecord territory) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.ParentTerritory.Id == territory.Id);
        }

        public int GetTerritoriesChildCount(TerritoryPart territory) {
            return _territoryPartRecord
                .Table
                .Where(tpr => tpr.ParentTerritory.Id == territory.Record.Id)
                .Count();
        }


        public int GetParentTerritoryId(int territoryId, int hierarchyId) {
            return _territoryPartRecord
              .Table
              .Where(tpr => tpr.TerritoryInternalRecord.Id == territoryId && tpr.Hierarchy.Id == hierarchyId)
              .Select(tpr=>tpr.ParentTerritory.Id)
              .SingleOrDefault();
        }

        private int? GetParentId(int territoryId, int hierarchyId, List<int> listIds) {
            var record = _territoryPartRecord
                .Table
                .Where(tpr => tpr.Id == territoryId && tpr.Hierarchy.Id == hierarchyId)
                .SingleOrDefault();

            if (record != null && record.TerritoryInternalRecord != null) {
                listIds.Add(record.TerritoryInternalRecord.Id);
            }

            if (record != null && record.ParentTerritory != null) { 
                return record.ParentTerritory.Id;
            }
            else {
                return null;
            }
        }

        public List<int> GetListOfParentIds(int territoryId, int hierarchyId, List<int> listIds) {
            var id = GetParentId(territoryId, hierarchyId, listIds);
            if (id == null || id == 0) {
                return listIds;
            }
            else {
                GetListOfParentIds(id??0, hierarchyId, listIds);
            }
            return listIds;
        }

    }
}
