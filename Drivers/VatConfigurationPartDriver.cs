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
using System.Web.Mvc;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPartDriver : ContentPartDriver<VatConfigurationPart> {

        private readonly IContentManager _contentManager;
        private readonly ITerritoriesService _territoriesService;
        private readonly IVatConfigurationService _vatConfigurationService;

        public VatConfigurationPartDriver(
            IContentManager contentManager,
            ITerritoriesService territoriesService,
            IVatConfigurationService vatConfigurationService) {

            _contentManager = contentManager;
            _territoriesService = territoriesService;
            _vatConfigurationService = vatConfigurationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "VatConfigurationPart"; }
        }

        protected override DriverResult Editor(VatConfigurationPart part, dynamic shapeHelper) {
            var model = CreateVM(part);
            return ContentShape("Parts_VatConfiguration_Edit", 
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/VatConfiguration",
                    Model: model,
                    Prefix: Prefix
                    ));
        }

        protected override DriverResult Editor(VatConfigurationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new VatConfigurationViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                part.Priority = model.Priority;
                part.TaxProductCategory = model.TaxProductCategory;
                // Check default category flag
                if (model.IsDefaultCategory) {
                    _vatConfigurationService.SetDefaultCategory(part);
                }
                part.DefaultRate = model.DefaultRate;
            }
            return Editor(part, shapeHelper);
        }

        private VatConfigurationViewModel CreateVM(VatConfigurationPart part) {
            // If no default VatConfigurationPart exists the GetDefaultCategoryId method returns 0,
            // as is defined in the interface. This way, the next new VatConfigurationPart that is created
            // will automatically becom the new default.
            var partIsDefault = part.ContentItem.Id == _vatConfigurationService.GetDefaultCategoryId();
            return new VatConfigurationViewModel {
                TaxProductCategory = part.TaxProductCategory,
                IsDefaultCategory = partIsDefault,
                DefaultRate = part.DefaultRate,
                Priority = part.Priority,
                Part = part
            };
        }


    }
}
