using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPartDriver : ContentPartDriver<FlexibleShippingMethodPart> {

        private readonly IEnumerable<IShippingAreaProvider> _shippingAreaProviders;

        public FlexibleShippingMethodPartDriver(
            IEnumerable<IShippingAreaProvider> shippingAreaProviders) {

            _shippingAreaProviders = shippingAreaProviders;
        }

        protected override string Prefix {
            get { return "FlexibleShippingMethodPart"; }
        }

        //GET
        protected override DriverResult Editor(
            FlexibleShippingMethodPart part, dynamic shapeHelper) {

            // Load applicability criteria

            return ContentShape("Parts_FlexibleShippingMethod_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/FlexibleShippingMethod",
                    Model: shapeHelper.ShippingEditor(
                        ShippingMethod: part,
                        ShippingAreas: _shippingAreaProviders.SelectMany(ap => ap.GetAreas()),
                        Prefix: Prefix),
                    Prefix: Prefix));
        }

        private class LocalViewModel {
            public string[] IncludedShippingAreas { get; set; }
            public string[] ExcludedShippingAreas { get; set; }
        }

        //POST
        protected override DriverResult Editor(
            FlexibleShippingMethodPart part, 
            IUpdateModel updater,
            dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, new[] { "IncludedShippingAreas", "ExcludedShippingAreas" });
            var dyn = new LocalViewModel();
            updater.TryUpdateModel(dyn, Prefix, new[] { "IncludedShippingAreas", "ExcludedShippingAreas" }, null);
            part.IncludedShippingAreas = dyn.IncludedShippingAreas == null
                ? ""
                : string.Join(",", dyn.IncludedShippingAreas);
            part.ExcludedShippingAreas = dyn.ExcludedShippingAreas == null
                ? ""
                : string.Join(",", dyn.ExcludedShippingAreas);
            return Editor(part, shapeHelper);
        }
    }
}
