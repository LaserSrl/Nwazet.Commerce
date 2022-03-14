using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingSiteSettingPart : ContentPart {
        public const string CacheKey = "FlexibleShippingSiteSettingsPart";

        public virtual bool EnablePriceTiers {
            get { return this.Retrieve(p => p.EnablePriceTiers); }
            set { this.Store(p => p.EnablePriceTiers, value); }
        }
    }
}
