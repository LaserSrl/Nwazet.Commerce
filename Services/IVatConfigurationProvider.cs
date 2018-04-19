using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    public interface IVatConfigurationProvider : ITaxProvider {
        /// <summary>
        /// Get all the published VAT Category configurations
        /// </summary>
        /// <returns></returns>
        IEnumerable<VatConfigurationPart> GetVatConfigurations();

        /// <summary>
        /// Updates the VAT category configurations for the given hierarchy.
        /// </summary>
        /// <param name="part">The HierarchyVatConfigurationPart whose information we are updating.</param>
        /// <param name="model">The HierarchyVatConfigurationPartViewModel object that contains the updated information.</param>
        void UpdateConfiguration(
            HierarchyVatConfigurationPart part, HierarchyVatConfigurationPartViewModel model);

        /// <summary>
        /// Updates the VAT category configurations for the given territory.
        /// </summary>
        /// <param name="part">The TerritoryVatConfigurationPart whose information we are updating.</param>
        /// <param name="model">The TerritoryVatConfigurationPartViewModel object that contains the updated information.</param>
        void UpdateConfiguration(
            TerritoryVatConfigurationPart part, TerritoryVatConfigurationPartViewModel model);
    }
}
