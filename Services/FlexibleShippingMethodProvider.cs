using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Collections.Generic;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodProvider : IShippingMethodProvider {
        private readonly IContentManager _contentManager;
        private Localizer T { get; set; }

        public FlexibleShippingMethodProvider(
            IContentManager contentManager) {

            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }

        public string Name { get { return T("Flexible Shipping Method").Text; } }

        public string ContentTypeName { get { return "FlexibleShippingMethod"; } }

        public IEnumerable<IShippingMethod> GetShippingMethods() {
            return _contentManager
                .Query<FlexibleShippingMethodPart, FlexibleShippingMethodRecord>()
                .ForVersion(VersionOptions.Published)
                .List();
        }
    }
}
