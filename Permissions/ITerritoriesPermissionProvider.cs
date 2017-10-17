using Orchard.ContentManagement.MetaData.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Permissions {
    public interface ITerritoriesPermissionProvider : IPermissionProvider {
        /// <summary>
        /// Returns the "Manage" permissions for all types of territory.
        /// </summary>
        /// <returns>The list of the dynamic permissions for the territories.</returns>
        /// <remarks>This method does not filter out the permissions that the current user does not have. 
        /// Rather, it returns all possible permissions for the types of territories.</remarks>
        IEnumerable<Permission> ListTerritoryTypePermissions();

        /// <summary>
        /// Returns the "Manage" permissions for all types of territory hierarchy.
        /// </summary>
        /// <returns>The list of the dynamic permissions for the territory hierarchies.</returns>
        /// <remarks>This method does not filter out the permissions that the current user does not have. 
        /// Rather, it returns all possible permissions for the types of territory hierarchies.</remarks>
        IEnumerable<Permission> ListHierarchyTypePermissions();

        /// <summary>
        /// Returns the dynamic permission computed for the type passed as parameter and considered
        /// as a Territory.
        /// </summary>
        /// <param name="typeDefinition">The type for whom the permission will be created.</param>
        /// <returns>The computed dynamic permission.</returns>
        Permission GetTerritoryPermission(ContentTypeDefinition typeDefinition);

        /// <summary>
        /// Returns the dynamic permission computed for the type passed as parameter and considered
        /// as a Hierarchy.
        /// </summary>
        /// <param name="typeDefinition">The type for whom the permission will be created.</param>
        /// <returns>The computed dynamic permission.</returns>
        Permission GetHierarchyPermission(ContentTypeDefinition typeDefinition);
    }
}
