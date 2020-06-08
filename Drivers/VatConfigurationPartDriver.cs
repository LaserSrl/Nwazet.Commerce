using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPartDriver : ContentPartDriver<VatConfigurationPart> {

        private readonly IContentManager _contentManager;
        private readonly ITerritoriesService _territoriesService;
        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public VatConfigurationPartDriver(
            IContentManager contentManager,
            ITerritoriesService territoriesService,
            IVatConfigurationService vatConfigurationService,
            IAuthorizer authorizer,
            INotifier notifier) {

            _contentManager = contentManager;
            _territoriesService = territoriesService;
            _vatConfigurationService = vatConfigurationService;
            _authorizer = authorizer;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "VatConfigurationPart"; }
        }

        protected override DriverResult Editor(VatConfigurationPart part, dynamic shapeHelper) {
            if (!_authorizer.Authorize(CommercePermissions.ManageTaxes)) {
                _notifier.Warning(T("Changes to the VAT configuration will not be saved because you don't have the correct permissions."));
            }
            var model = CreateVM(part);
            var configIssues = CheckIntersectionsBetweenHierarchies(part);
            if (configIssues.Any()) {
                var sb = new StringBuilder();
                sb.AppendLine(T("There are issues with the hierarchies configured for this VAT:").Text);
                foreach (var issue in configIssues) {
                    sb.AppendLine(T("\t{0} and {1} intersect on {2}", issue.Hierarchy1, issue.Hierarchy2, issue.Territory).Text);
                }
                _notifier.Warning(T("{0}", sb.ToString()));
            }
            return ContentShape("Parts_VatConfiguration_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/VatConfiguration",
                    Model: model,
                    Prefix: Prefix
                    ));
        }

        protected override DriverResult Editor(VatConfigurationPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (_authorizer.Authorize(CommercePermissions.ManageTaxes)) {
                var model = new VatConfigurationPartViewModel();
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                    part.Priority = model.Priority;
                    part.TaxProductCategory = model.TaxProductCategory;
                    // Check default category flag
                    if (model.IsDefaultCategory) {
                        _vatConfigurationService.SetDefaultCategory(part);
                    }
                    part.DefaultRate = model.DefaultRate;
                }
            }
            return Editor(part, shapeHelper);
        }

        private VatConfigurationPartViewModel CreateVM(VatConfigurationPart part) {
            // If no default VatConfigurationPart exists the GetDefaultCategoryId method returns 0,
            // as is defined in the interface. This way, the next new VatConfigurationPart that is created
            // will automatically becom the new default.
            var partIsDefault = part.ContentItem.Id == _vatConfigurationService.GetDefaultCategoryId();
            return new VatConfigurationPartViewModel {
                TaxProductCategory = part.TaxProductCategory,
                IsDefaultCategory = partIsDefault,
                DefaultRate = part.DefaultRate,
                Priority = part.Priority,
                Part = part,
                ItemizedSummary = BuildSummary(part)
            };
        }

        private List<VatConfigurationHierarchySummaryViewModel> BuildSummary(VatConfigurationPart part) {
            var summary = new List<VatConfigurationHierarchySummaryViewModel>();
            if (part.Hierarchies == null || !part.Hierarchies.Any()) {
                return summary;
            }
            foreach (var hierarchyConfig in part.Hierarchies) {
                var subRegions = new List<VatConfigurationTerritorySummaryViewModel>();
                var territories = part.Territories
                    .Where(tpd => tpd.Item1.Record.Hierarchy.Id == hierarchyConfig.Item1.Record.Id);
                // build the tree of these territories
                var allRegions = new List<VatConfigurationTerritorySummaryViewModel>();
                foreach (var territoryConfig in territories) {
                    // build a view model for the territory
                    var vm = new VatConfigurationTerritorySummaryViewModel {
                        Name = _contentManager.GetItemMetadata(territoryConfig.Item1).DisplayText,
                        Part = territoryConfig.Item1,
                        Item = territoryConfig.Item1.ContentItem,
                        Rate = territoryConfig.Item2
                    };
                    // add to the temporary list of all TerritoryVMs
                    allRegions.Add(vm);
                    subRegions.Add(vm);
                }
                // for each current subregion (these are the ones that have a configured rate)
                // build its tree towards the root (the hierarchy). We can do this by
                // building the vm for the parent if it is missing.
                foreach (var region in subRegions) {
                    var parentRecord = region.Part.Record.ParentTerritory;
                    var current = region;
                    while (parentRecord != null) {
                        // find the parent among the existing regions if it's there
                        var parent = allRegions
                            .FirstOrDefault(r => r.Part.Record.Id == parentRecord.Id);
                        if (parent != null) {
                            parent.SubRegions.Add(current);
                            // exit condition from the loop
                            parentRecord = null;
                        } else {
                            // get the part from the record
                            var parentPart = _contentManager
                                .Get<TerritoryPart>(parentRecord.Id);
                            parent = new VatConfigurationTerritorySummaryViewModel {
                                Name = _contentManager.GetItemMetadata(parentPart).DisplayText,
                                Part = parentPart,
                                Item = parentPart.ContentItem,
                                Rate = -1 // negative rate as special case marker
                            };
                            parent.SubRegions.Add(current);
                            allRegions.Add(parent);
                            // check its parent next
                            current = parent;
                            parentRecord = parentRecord.ParentTerritory;
                        }

                    }
                }

                summary.Add(
                    new VatConfigurationHierarchySummaryViewModel {
                        Name = _contentManager.GetItemMetadata(hierarchyConfig.Item1).DisplayText,
                        Item = hierarchyConfig.Item1.ContentItem,
                        Rate = hierarchyConfig.Item2,
                        SubRegions = allRegions
                            // the top level
                            .Where(vm => vm.Part.Record.ParentTerritory == null)
                            .ToList()
                    });
            }
            return summary;
        }

        private IEnumerable<HierarchyIntersection> CheckIntersectionsBetweenHierarchies(VatConfigurationPart part) {
            
            if (part.Hierarchies != null
                && part.Hierarchies.Count() > 1) {
                // get the TerritoryHierarchyParts
                var hierarchies = part.Hierarchies.Select(tup => tup.Item1).ToArray();
                for (int i = 0; i < hierarchies.Length-1; i++) {
                    // territories from "first" hierarchy
                    var source = hierarchies[i]
                        .Record.Territories; //.Select(ci => ci.As<TerritoryPart>());
                    // name of first hierarchy
                    var sourceString = _contentManager.GetItemMetadata(hierarchies[i]).DisplayText;
                    for (int j = i+1; j < hierarchies.Length; j++) {
                        // territories from second hierarchy
                        var other = hierarchies[j]
                            .Record.Territories; //.Select(ci => ci.As<TerritoryPart>());
                        // name of second hierarchy
                        var otherString = _contentManager.GetItemMetadata(hierarchies[j]).DisplayText;
                        // intersection of the sets of territories
                        var intersection = source.Intersect(other, new TerritoryPartRecord.TerritoryPartRecordComparer());
                        if (intersection.Any()) {
                            foreach (var territory in intersection) {

                                yield return new HierarchyIntersection {
                                    Hierarchy1 = sourceString,
                                    Hierarchy2 = otherString,
                                    Territory = territory.TerritoryInternalRecord.Name
                                };
                            }
                        }
                    }
                }
            }

            yield break;
        }

        class HierarchyIntersection {
            public string Hierarchy1 { get; set; }
            public string Hierarchy2 { get; set; }
            public string Territory { get; set; }
        }

        protected override void Exporting(VatConfigurationPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element
                .With(part)
                .ToAttr(p => p.Priority)
                .ToAttr(p => p.TaxProductCategory)
                .ToAttr(p => p.DefaultRate);
        }

        protected override void Importing(VatConfigurationPart part, ImportContentContext context) {
            var element = context.Data.Element(part.PartDefinition.Name);
            if (element == null) {
                return;
            }
            element
               .With(part)
               .FromAttr(p => p.Priority)
               .FromAttr(p => p.TaxProductCategory)
               .FromAttr(p => p.DefaultRate);
        }
    }
}
