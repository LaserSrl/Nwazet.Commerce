using Nwazet.Commerce.Exceptions;
using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface ITerritoriesHierarchyService : IDependency {
        /// <summary>
        /// Assigns the territory to the hierarchy, at its root level, and also assigns
        /// all child-territories to the hierarchy. This method will fail if the ContentItem
        /// for the TerritoryPart is not of a type corresponding to the one allowed for the
        /// hierarchy. This method allows moving a territory between two hierarchies.
        /// </summary>
        /// <param name="territory">The territory being assigned to the hierarchy.</param>
        /// <param name="hierarchy">The hierarchy the territory is being assigned to.</param>
        /// <exception cref="ArrayTypeMismatchException">Throws an ArrayTypeMismatchException
        /// if the type for the territory does not match the TerritoryType expected for the 
        /// hierarchy.</exception>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if either
        /// of the arguments is null or has a null underlying record.</exception>
        void AddTerritory(TerritoryPart territory, TerritoryHierarchyPart hierarchy);

        /// <summary>
        /// Assigns a new parent to a territory. Both the parent and the territory must be of a same
        /// ContentType, as well as belong to the same hierarchy.
        /// </summary>
        /// <param name="territory">The territory being moved under a new parent.</param>
        /// <param name="parent">The parent territory.</param>
        /// <exception cref="ArrayTypeMismatchException">Throws an ArrayTypeMismatchException
        /// if the type for the territory does not match the type of the prospective parent.</exception>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if either
        /// of the arguments is null or has a null underlying record.</exception>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if either
        /// of the arguments' Hierarchies is null.</exception>
        /// <exception cref="ArrayTypeMismatchException">Throws an ArrayTypeMismatchException
        /// if the two arguments belong to different Hierarchies.</exception>
        void AssignParent(TerritoryPart territory, TerritoryPart parent);

        /// <summary>
        /// Assigns the territory to the hierarchy, under the specified parent, and also assigns
        /// all child-territories to the hierarchy. This method will fail if the ContentItem
        /// for the TerritoryPart is not of a type corresponding to the one allowed for the
        /// hierarchy. This method allows moving a territory between two hierarchies.
        /// </summary>
        /// <param name="territory">The territory being assigned to the hierarchy.</param>
        /// <param name="hierarchy">The hierarchy the territory is being assigned to.</param>
        /// <param name="parent">The parent territory. If the parent is null, the territory will be
        /// positioned at the root level of the hierarchy.</param>
        /// <remarks>Using this method should be faster than calling AddTerritory(...) followed by 
        /// AssignParent(...), because several checks may be skipped.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Throws an ArrayTypeMismatchException
        /// if the type for the territory does not match the TerritoryType expected for the 
        /// hierarchy.</exception>
        /// <exception cref="ArrayTypeMismatchException">Throws an ArrayTypeMismatchException
        /// if the parent belong to a different hierarchy.</exception>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if either
        /// of the arguments is null or has a null underlying record.</exception>
        void AddTerritory(TerritoryPart territory, TerritoryHierarchyPart hierarchy, TerritoryPart parent);

        /// <summary>
        /// Assigns the TerritoryInternalRecord identified by the Name given as argument as underlying
        /// territory for the TerritoryPart.
        /// </summary>
        /// <param name="territory">The TerritoryPart whose internal record we are trying to assign.</param>
        /// <param name="name">The name of a TerritoryInternalRecord.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if no TerritoryInternalRecord
        /// was found with the given name.</exception>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the territory argument
        /// is null.</exception>
        /// <exception cref="TerritoryInternalDuplicateException">Throws a TerritoryInternalDuplicateException 
        /// if the TerritoryInternalRecord has already been assigned to a different territory in the same hierarchy.</exception>
        void AssignInternalRecord(TerritoryPart territory, string name);

        /// <summary>
        /// Assigns the TerritoryInternalRecord identified by the Id given as argument as underlying
        /// territory for the TerritoryPart.
        /// </summary>
        /// <param name="territory">The TerritoryPart whose internal record we are trying to assign.</param>
        /// <param name="id">The id of a TerritoryInternalRecord.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if no TerritoryInternalRecord
        /// was found with the given Id.</exception>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if the territory argument
        /// is null.</exception>
        /// <exception cref="TerritoryInternalDuplicateException">Throws a TerritoryInternalDuplicateException 
        /// if the TerritoryInternalRecord has already been assigned to a different territory in the same hierarchy.</exception>
        void AssignInternalRecord(TerritoryPart territory, int id);

        /// <summary>
        /// Assigns the TerritoryInternalRecord given as argument as underlying territory for the TerritoryPart.
        /// </summary>
        /// <param name="territory">The TerritoryPart whose internal record we are trying to assign.</param>
        /// <param name="internalRecord">The TerritoryInternalRecord.</param>
        /// <exception cref="ArgumentNullException">Throws an ArgumentNullException if either argument
        /// is null.</exception>
        /// <exception cref="TerritoryInternalDuplicateException">Throws a TerritoryInternalDuplicateException 
        /// if the TerritoryInternalRecord has already been assigned to a different territory in the same hierarchy.</exception>
        void AssignInternalRecord(TerritoryPart territory, TerritoryInternalRecord internalRecord);
    }
}
