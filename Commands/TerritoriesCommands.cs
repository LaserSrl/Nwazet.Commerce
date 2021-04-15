using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Commands;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Commands {
    [OrchardFeature("Territories")]
    public class TerritoriesCommands : DefaultOrchardCommandHandler {

        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly IRepository<TerritoryPartRecord> _territoryPartRepository;
        private readonly ITransactionManager _transactionManager;

        public TerritoriesCommands(
            ITerritoriesRepositoryService territoriesRepositoryService,
            IRepository<TerritoryPartRecord> territoryPartRepository,
            ITransactionManager transactionManager) {
            
            _territoriesRepositoryService = territoriesRepositoryService;
            _territoryPartRepository = territoryPartRepository;
            _transactionManager = transactionManager;
        }

        [CommandName("territories import")]
        [CommandHelp("territories import <name>\r\n\t" + "Imports the territory with the given name to be used as a reference.")]
        public void Import(string territoryName) {
            _territoriesRepositoryService.TryAddTerritory(territoryName);
        }

        [CommandName("territories calculatefullpath")]
        [CommandHelp("territories calculatefullpath\r\n\t" + "Recalculate TerritoryPart.TerritoriesFullPath.")]
        public void CalculateFullPath() {
            var territoryItems = _territoryPartRepository.Table.ToList();
            var firstLevel = territoryItems.Where(x => x.ParentTerritory == null || x.ParentTerritory.Id == 0);
            foreach (var item in firstLevel) {
                UpdatePath(0, item.Hierarchy != null ? item.Hierarchy.Id : 0, "\\", territoryItems);
            }
        }

        // this method is same as \Nwazet.Commerce\Migrations\TerritoriesMigrations.cs
        //TODO: maybe it should be better to have a service for that
        private void UpdatePath(int parentId, int hierarchyId, string parentPath, IEnumerable<TerritoryPartRecord> territoryItems) {
            if (hierarchyId <= 0) {
                return;
            }
            Func<TerritoryPartRecord, bool> condition;
            if (parentId == 0) {
                condition = x => x.ParentTerritory == null || x.ParentTerritory.Id == parentId;
            }
            else {
                condition = x => x.ParentTerritory != null && x.ParentTerritory.Id == parentId;
            }
            if (!territoryItems.Any(condition)) {
                return;
            }
            var current = _transactionManager.GetSession();
            string sqlUpdate;
            if (parentId > 0) {
                sqlUpdate = "UPDATE Nwazet.Commerce.Models.TerritoryPartRecord t SET t.TerritoriesFullPath=concat('" + parentPath.Replace("'", "''") + "', trim(str(t.id)), '\\') WHERE isnull(t.Hierarchy.Id,0)=" + hierarchyId + " and isnull(t.ParentTerritory.Id,0)=" + parentId;
            }
            else {
                sqlUpdate = "UPDATE Nwazet.Commerce.Models.TerritoryPartRecord t SET t.TerritoriesFullPath=concat('" + parentPath.Replace("'", "''") + "', trim(str(t.id)), '\\') WHERE isnull(t.Hierarchy.Id,0)=" + hierarchyId + " and isnull(t.ParentTerritory.Id,0)=0";
            }
            var bulkUpdate = current.CreateQuery(sqlUpdate);
            bulkUpdate.ExecuteUpdate();
            var children = territoryItems.Where(condition);
            foreach (var child in children) {
                if (child.Hierarchy == null) {
                    continue;
                }
                UpdatePath(child.Id, child.Hierarchy.Id, parentPath + child.Id + "\\", territoryItems);
            }
        }

    }
}




