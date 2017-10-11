using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface ITerritoriesService : IDependency {
        /// <summary>
        /// Returns the type definitions for the types of territory hierarchies that
        /// the user is allowed to manage.
        /// </summary>
        /// <returns>An Enumerable of the ContentTypeDefinitions for all types 
        /// that the current user is allowed to manage.</returns>
        IEnumerable<ContentTypeDefinition> GetHierarchyTypes();

        /// <summary>
        /// Returns the "Manage" permissions for all types of territory hierarchy.
        /// </summary>
        /// <returns>The list of the dynamic permissions for the territory hierarchies.</returns>
        /// <remarks>This method does not filter out the permissions that the current user does not have. 
        /// Rather, it returns all possible permissions for the types of territory hierarchies.</remarks>
        IEnumerable<Permission> ListHierarchyTypePermissions();

        /// <summary>
        /// Returns the type definitions for the types of territories that
        /// the user is allowed to manage.
        /// </summary>
        /// <returns>An Enumerable of the ContentTypeDefinitions for all types 
        /// that the current user is allowed to manage</returns>
        IEnumerable<ContentTypeDefinition> GetTerritoryTypes();

        /// <summary>
        /// Returns the "Manage" permissions for all types of territory.
        /// </summary>
        /// <returns>The list of the dynamic permissions for the territories.</returns>
        /// <remarks>This method does not filter out the permissions that the current user does not have. 
        /// Rather, it returns all possible permissions for the types of territories.</remarks>
        IEnumerable<Permission> ListTerritoryTypePermissions();

        /// <summary>
        /// Provides an IContentQuery for the Latest versions of TerritoryHierarchyParts
        /// </summary>
        /// <returns></returns>
        IContentQuery<TerritoryHierarchyPart, TerritoryHierarchyPartRecord> GetHierarchiesQuery();

        /// <summary>
        /// Provides an IContentQuery for the specific versions of TerritoryHierarchyParts
        /// </summary>
        /// <param name="versionOptions">The version for the items. Defaults at Latest.</param>
        /// <returns></returns>
        IContentQuery<TerritoryHierarchyPart, TerritoryHierarchyPartRecord> GetHierarchiesQuery(VersionOptions versionOptions);

        /// <summary>
        /// Provides an IContentQuery for the TerritoryParts in a given hierarchy. The version of the territories is the
        /// version of the hierarchy, or Latest.
        /// </summary>
        /// <param name="hierarchyPart">The hierarchy that the territories belong to.</param>
        /// <returns></returns>
        IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(TerritoryHierarchyPart hierarchyPart);

        /// <summary>
        /// Provides an IContentQuery for the TerritoryParts in a given hierarchy.
        /// </summary>
        /// <param name="hierarchyPart">The hierarchy that the territories belong to.</param>
        /// <param name="versionOptions">The version for the items. Defaults to the version of the item of the hierarchyPart,
        /// falling back to Latest.</param>
        /// <returns></returns>
        IContentQuery<TerritoryPart, TerritoryPartRecord> GetTerritoriesQuery(TerritoryHierarchyPart hierarchyPart, VersionOptions versionOptions);
    }
}
