using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Orchard.ContentManagement;
using Nwazet.Commerce.ViewModels;
using Nwazet.Commerce.Services;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class ProductVatConfigurationPartDriver : ContentPartDriver<ProductVatConfigurationPart> {

        private readonly IVatConfigurationProvider _vatConfigurationProvider;
        private readonly IContentManager _contentManager;

        public ProductVatConfigurationPartDriver(
            IVatConfigurationProvider vatConfigurationProvider,
            IContentManager contentManager) {

            _vatConfigurationProvider = vatConfigurationProvider;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }


        public Localizer T;

        protected override string Prefix {
            get { return "ProductVatConfigurationPart"; }
        }
        
        protected override DriverResult Editor(ProductVatConfigurationPart part, dynamic shapeHelper) {
            var model = CreateVM(part);
            return ContentShape("Parts_ProductVatConfiguration_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/ProductVatConfiguration",
                    Model: model,
                    Prefix: Prefix
                    ));
        }

        protected override DriverResult Editor(ProductVatConfigurationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ProductVatConfigurationPartEditorViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                // set the vat category
                if (model.VatConfigurationId == 0) {
                    part.Record.VatConfiguration = null;
                } else if (model.VatConfigurationId > 0) {
                    part.Record.VatConfiguration = _contentManager
                        .Get(model.VatConfigurationId) // will be null if that ContentItem is not Published
                        ?.As<VatConfigurationPart>() // will be null if that ContentItem is not VatConfigurationPart
                        ?.Record;
                }
            }
            return Editor(part, shapeHelper);
        }

        private ProductVatConfigurationPartEditorViewModel CreateVM(
            ProductVatConfigurationPart part) {
            return new ProductVatConfigurationPartEditorViewModel(T("Default").Text) {
                VatConfigurationId = part.UseDefaultVatCategory 
                    ? 0 
                    : part.VatConfigurationPart.Record.Id,
                AllVatConfigurations = _vatConfigurationProvider
                    .GetVatConfigurations()
                    .ToList()
            };
        }
    }
}
