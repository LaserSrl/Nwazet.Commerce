using Nwazet.Commerce.Controllers;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Linq;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationSiteSettingsPartDriver : ContentPartDriver<VatConfigurationSiteSettingsPart> {

        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly IContentManager _contentManager;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public VatConfigurationSiteSettingsPartDriver(
            IVatConfigurationService vatConfigurationService,
            IContentManager contentManager,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _vatConfigurationService = vatConfigurationService;
            _contentManager = contentManager;
            _territoriesRepositoryService = territoriesRepositoryService;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "VatConfigurationSiteSettings"; }
        }

        protected override DriverResult Editor(VatConfigurationSiteSettingsPart part, dynamic shapeHelper) {
            return ContentShape("SiteSettings_VatConfiguration",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "SiteSettings/VatConfiguration",
                    Model: CreateVM(part),
                    Prefix: Prefix
                    )
                ).OnGroup("ECommerceSiteSettings");
        }

        protected override DriverResult Editor(VatConfigurationSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new VatConfigurationSiteSettingsPartViewModel();
            if (updater is ECommerceSettingsAdminController
                && updater.TryUpdateModel(model, Prefix, null, null)) {

                part.DefaultTerritoryForVatId = model.DefaultTerritoryForVatId;
            }
            return Editor(part, shapeHelper);
        }

        private VatConfigurationSiteSettingsPartViewModel CreateVM(VatConfigurationSiteSettingsPart part) {
            return new VatConfigurationSiteSettingsPartViewModel(T("None").Text) {
                DefaultVatConfigurationPart = _vatConfigurationService
                    .GetDefaultCategory(),
                DefaultTerritoryForVatId = part.DefaultTerritoryForVatId,
                AvailableTerritoryInternalRecords = _territoriesRepositoryService
                    .GetTerritories()
                    .ToList()
                    .CreateSafeDuplicate()
                    .ToList()
            };
        }


        protected override void Exporting(VatConfigurationSiteSettingsPart part, ExportContentContext context) {
            // To export DefaultVatConfigurationId, taking inspiration from 
            // Laser.Orchard.NwazetIntegration.Drivers.AddressConfigurationSiteSettingsPartDriver
            var root = context.Element(part.PartDefinition.Name);

            var vat = _contentManager
                .Get<VatConfigurationPart>(part.DefaultVatConfigurationId);
            var vatIdentity = _contentManager
                .GetItemMetadata(vat).Identity;
            root.SetAttributeValue("DefaultVatConfigurationIdentity", vatIdentity);

            // Since territory Id is the id on the database (it may change from tenant to tenant), 
            // I need to export the Name of the territory, which is unique.
            var territory = _territoriesRepositoryService.GetTerritoryInternal(part.DefaultTerritoryForVatId);
            root.SetAttributeValue("DefaultTerritoryForVatName", territory.Name);
        }

        protected override void Imported(VatConfigurationSiteSettingsPart part, ImportContentContext context) {
            // To import DefaultVatConfigurationId, taking inspiration from 
            // Laser.Orchard.NwazetIntegration.Drivers.AddressConfigurationSiteSettingsPartDriver
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) return;

            context.ImportAttribute(
                part.PartDefinition.Name,
                "DefaultVatConfigurationIdentity",
                vatId =>
                    part.DefaultVatConfigurationId = context.GetItemFromSession(vatId).Id);

            // I get the correct territory id by consulting the repository service with the name I have exported, which should be unique.
            // The service takes care of hashing it.
            context.ImportAttribute(
                part.PartDefinition.Name,
                "DefaultTerritoryForVatName",
                territoryName =>
                    part.DefaultTerritoryForVatId = _territoriesRepositoryService.GetTerritoryInternal(territoryName).Id);
        }
    }
}
