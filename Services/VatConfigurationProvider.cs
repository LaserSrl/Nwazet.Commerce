using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationProvider : ITaxProvider, IVatConfigurationProvider {

        private readonly IContentManager _contentManager;
        private readonly IRepository<HierarchyVatConfigurationIntersectionRecord> _hierarchyVatConfigurations;
        private readonly IRepository<TerritoryVatConfigurationIntersectionRecord> _territoryVatConfigurations;

        private Localizer T { get; set; }

        public VatConfigurationProvider(
            IContentManager contentManager,
            IRepository<HierarchyVatConfigurationIntersectionRecord> hierarchyVatConfigurations,
            IRepository<TerritoryVatConfigurationIntersectionRecord> territoryVatConfigurations) {

            _contentManager = contentManager;
            _hierarchyVatConfigurations = hierarchyVatConfigurations;
            _territoryVatConfigurations = territoryVatConfigurations;

            T = NullLocalizer.Instance;
        }

        public string ContentTypeName {
            get { return "VATConfiguration"; }
        }

        public string Name {
            get { return T("VAT Category Configuration").Text; }
        }

        public IEnumerable<ITax> GetTaxes() {
            return GetVatConfigurations();
        }

        public IEnumerable<VatConfigurationPart> GetVatConfigurations() {
            return _contentManager
                .Query<VatConfigurationPart, VatConfigurationPartRecord>()
                .ForVersion(VersionOptions.Published)
                .List();
        }

        public void UpdateConfiguration(
            HierarchyVatConfigurationPart part, HierarchyVatConfigurationPartViewModel model) {
            // fetch all configurations for the part here to avoid fetching them one by one as needed
            var allConfigurations = _hierarchyVatConfigurations
                        .Fetch(r => r.Hierarchy == part.Record);
            foreach (var detailVM in model.AllVatConfigurations) {
                var oldCfg = part.VatConfigurations
                    .FirstOrDefault(tup => tup.Item1.Record.Id == detailVM.VatConfigurationPartId);
                if (oldCfg == null) {
                    if (detailVM.IsSelected) {
                        // we added a new VAT category configuration to this hiehrarchy
                        _hierarchyVatConfigurations.Create(
                            new HierarchyVatConfigurationIntersectionRecord {
                                Hierarchy = part.Record,
                                VatConfiguration = _contentManager
                                    .Get<VatConfigurationPart>(detailVM.VatConfigurationPartId)
                                    .Record,
                                Rate = detailVM.Rate.HasValue ? detailVM.Rate.Value : 0
                            });
                    }
                } else {
                    var intersection = allConfigurations.FirstOrDefault(r => r.VatConfiguration == oldCfg.Item1.Record);
                    if (detailVM.IsSelected) {
                        // update Rate if different
                        if (oldCfg.Item2 != detailVM.Rate) {
                            intersection.Rate = detailVM.Rate.HasValue ? detailVM.Rate.Value : 0;
                            _hierarchyVatConfigurations.Update(intersection);
                        }
                    } else {
                        // we removed a VAT category configuration
                        _hierarchyVatConfigurations.Delete(intersection);
                    }
                }
            }
        }
    }
}
