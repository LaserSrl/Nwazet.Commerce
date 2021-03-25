using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingSiteSettingPartHandler : ContentHandler{

        public CouponingSiteSettingPartHandler() {
            Filters.Add(new ActivatingFilter<CouponingSiteSettingPart>("Site"));
        }
    }
}
