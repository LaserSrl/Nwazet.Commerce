using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationHierarchySummaryViewModel {

        public VatConfigurationHierarchySummaryViewModel() {
            SubRegions = new List<VatConfigurationTerritorySummaryViewModel>();
        }

        public string Name { get; set; }
        public ContentItem Item { get; set; }
        public decimal Rate { get; set; }
        public List<VatConfigurationTerritorySummaryViewModel> SubRegions { get; set; }
    }
}
