using Nwazet.Commerce.Models;
using Orchard;
using System.Collections.Generic;

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
        List<TerritoryPartRecord> GetHierarchyTerritories(TerritoryHierarchyPart hierarchy);

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
        List<TerritoryPartRecord> GetTerritoriesChild(TerritoryPart territory);
    }
}
