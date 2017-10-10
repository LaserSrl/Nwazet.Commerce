using Orchard;
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
    }
}
