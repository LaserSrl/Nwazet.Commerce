using Orchard.ContentManagement.Records;

namespace Nwazet.Commerce.Models {
    public class WeightBasedShippingMethodPartRecord : ContentPartRecord {
        public WeightBasedShippingMethodPartRecord() {
            MinimumWeight = 0;
            MaximumWeight = null;
        }

        public virtual string Name { get; set; }
        public virtual string ShippingCompany { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal? MinimumWeight { get; set; }
        public virtual decimal? MaximumWeight { get; set; } // Set to double.PositiveInfinity (the default) for unlimited weight ranges
        public virtual string IncludedShippingAreas { get; set; }
        public virtual string ExcludedShippingAreas { get; set; }
    }
}
