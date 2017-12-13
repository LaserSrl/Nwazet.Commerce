using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationProvider : ITaxProvider {

        private readonly IContentManager _contentManager;
        private Localizer T { get; set; }

        public VatConfigurationProvider(
            IContentManager contentManager) {

            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public string ContentTypeName {
            get { return T("AdvancedVAT").Text; }
        }

        public string Name {
            get { return T("Advanced VAT Configuration").Text; }
        }

        public IEnumerable<ITax> GetTaxes() {
            return _contentManager
                .Query<VatConfigurationPart, VatConfigurationPartRecord>()
                .ForVersion(VersionOptions.Published)
                .List();
        }
    }
}
