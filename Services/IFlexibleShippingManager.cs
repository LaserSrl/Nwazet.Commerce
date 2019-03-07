using Nwazet.Commerce.ApplicabilityCriteria;
using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Orchard;
using System.Collections.Generic;

namespace Nwazet.Commerce.Services {
    public interface IFlexibleShippingManager : IDependency {
        /// <summary>
        /// Gets the descriptros of all avaiable ApplicationCriteria
        /// </summary>
        /// <returns></returns>
        IEnumerable<TypeDescriptor<ApplicabilityCriterionDescriptor>> DescribeCriteria();

        /// <summary>
        /// Gets the descriptor for a specific ApplicabilityCriterion, identified by
        /// its Category and Type.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ApplicabilityCriterionDescriptor GetCriteria(
            string category, string type);

        /// <summary>
        /// Test all ApplicabilityCriteria for the shipping method with the given Id and
        /// at the conditions in the context.
        /// </summary>
        /// <param name="methodId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        bool TestCriteria(int methodId, ApplicabilityContext context);

        /// <summary>
        /// Deletes all ApplicabilityCriteria for the shippping method with the given Id.
        /// </summary>
        /// <param name="methodId"></param>
        void DeleteAllCriteria(int methodId);

        /// <summary>
        /// Delets the ApplicabilityCriterion with the given Id.
        /// </summary>
        /// <param name="criterionId"></param>
        void DeleteCriterion(int criterionId);
    }
}
