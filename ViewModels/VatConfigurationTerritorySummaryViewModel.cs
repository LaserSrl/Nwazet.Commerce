using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationTerritorySummaryViewModel {
        public VatConfigurationTerritorySummaryViewModel() {
            SubRegions = new List<VatConfigurationTerritorySummaryViewModel>();
        }

        public string Name { get; set; }
        public TerritoryPart Part { get; set; }
        public ContentItem Item { get; set; }
        public decimal Rate { get; set; }

        public List<VatConfigurationTerritorySummaryViewModel> SubRegions { get; set; }
    }
}
