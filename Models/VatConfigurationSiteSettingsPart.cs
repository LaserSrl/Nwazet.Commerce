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
        public string DefaultCategory {
            get { return this.Retrieve(p => p.DefaultCategory); }
            set { this.Store(p => p.DefaultCategory, value); }
        }
    }
}
