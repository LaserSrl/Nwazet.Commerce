using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System.Globalization;

namespace Nwazet.Commerce.Settings.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ContentFieldCombinationWeldingSettings {
        public bool WeldToCombination { get; set; }

        public ContentFieldCombinationWeldingSettings() {
            WeldToCombination = false;
        }

        public static void SetValues(ContentPartFieldDefinitionBuilder builder, bool weldToCombination) {
            builder.WithSetting("ContentFieldCombinationWeldingSettings.WeldToCombination", weldToCombination.ToString(CultureInfo.InvariantCulture));
        }
    }
}
