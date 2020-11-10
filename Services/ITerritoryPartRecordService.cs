using Nwazet.Commerce.Models;
using Orchard;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Services {
    public interface ITerritoryPartRecordService : IDependency {
        /// <summary>
        /// given the hierarchy returns the number of territories
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        int GetHierarchyTerritoriesCount(TerritoryHierarchyPart hierarchy);

        /// <summary>
        /// given the hierarchy returns the list of territories
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        IQueryable<TerritoryPartRecord> GetHierarchyTerritories(TerritoryHierarchyPart hierarchy);

        /// <summary>
        /// given the territory returns the number of child of territories
        /// </summary>
        /// <param name="territory"></param>
        /// <returns></returns>
        int GetTerritoriesChildCount(TerritoryPart territory);

        /// <summary>
        /// given the territory returns the list child of territories
        /// </summary>
        /// <param name="territory"></param>
        /// <returns></returns>
        IQueryable<TerritoryPartRecord> GetTerritoriesChild(TerritoryPart territory);

        /// <summary>
        /// given the territory record returns the list child of territories
        /// </summary>
        /// <param name="territory"></param>
        /// <returns></returns>
        IQueryable<TerritoryPartRecord> GetTerritoriesChild(TerritoryPartRecord territory);

        /// <summary>
        /// return id of parent id
        /// </summary>
        /// <param name="territoryId"></param>
        /// <param name="hierarchyId"></param>
        /// <returns></returns>
        int GetParentTerritoryId(int territoryId, int hierarchyId);

        /// <summary>
        /// return list of ids 
        /// </summary>
        /// <param name="territoryId"></param>
        /// <param name="hierarchyId"></param>
        /// <param name="listIds"></param>
        /// <returns></returns>
        List<int> GetListOfParentIds(int territoryId, int hierarchyId, List<int> listIds);
    }
}
