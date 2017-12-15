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

        public VatConfigurationPartDriver(
            IContentManager contentManager,
            ITerritoriesService territoriesService) {

            _contentManager = contentManager;
            _territoriesService = territoriesService;

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
                part.Category = model.Category;
                // Check default category flag like it's done for homepage
                part.DefaultRate = model.DefaultRate;
                var hierarchy = _contentManager.Get(model.SelectedHierarchyId, VersionOptions.Latest);
                if (hierarchy == null || hierarchy.As<TerritoryHierarchyPart>() == null) {
                    // A hierarchy must be selected
                    updater.AddModelError("Selected Hierarchy", T("You need to select a valid hierarchy."));
                } else {
                    part.Hierarchy = hierarchy;
                }
            }
            return Editor(part, shapeHelper);
        }

        private VatConfigurationViewModel CreateVM(VatConfigurationPart part) {
            return new VatConfigurationViewModel {
                Category = part.Category, 
                IsDefaultCategory = part.IsDefaultCategory,
                DefaultRate = part.DefaultRate,
                Priority = part.Priority,
                SelectedHierarchyId = part.Hierarchy?.Id ?? -1,
                SelectedHierarchyText = part.Hierarchy == null ? string.Empty 
                    : _contentManager.GetItemMetadata(part.Hierarchy).DisplayText,
                SelectedHierarchyItem = part.Hierarchy,
                Part = part,
                Hierarchies = _territoriesService
                    .GetHierarchiesQuery()
                    .List()
                    .Select(thp => new SelectListItem {
                        Selected = (part.Hierarchy?.Id ?? 0) == thp.Id,
                        Text = _contentManager.GetItemMetadata(thp.ContentItem).DisplayText,
                        Value = thp.Id.ToString()
                    })
            };
        }
    }
}
