using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingSiteSettingViewModel {
        public bool EnablePriceTiers { get; set; }
    }
}
