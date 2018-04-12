using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationSiteSettingsPart : ContentPart {

        public int DefaultVatConfigurationId {
            get { return this.Retrieve(p => p.DefaultVatConfigurationId); }
            set { this.Store(p => p.DefaultVatConfigurationId, value); }
        }
    }
}
