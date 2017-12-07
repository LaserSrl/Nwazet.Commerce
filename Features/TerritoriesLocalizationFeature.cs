using Orchard.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;
using Orchard.ContentManagement.MetaData;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Features {
    public class TerritoriesLocalizationFeature : IFeatureEventHandler {

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TerritoriesLocalizationFeature(
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;
        }

        public void Enabled(Feature feature) {
            if (feature.Descriptor.Id == "Territories.Localization") {
                // Look for all ContentTypes that have a TerritoryHierarrchyPart
                var hierarchyTypes = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(ctd => ctd
                        .Parts
                        .Any(pa => pa
                            .PartDefinition
                            .Name
                            .Equals(TerritoryHierarchyPart.PartName, StringComparison.InvariantCultureIgnoreCase)));
                if (hierarchyTypes.Any(ht => ht
                    .Parts
                    .Any(ctpd => ctpd
                        .PartDefinition
                        .Name
                        .Equals("LocalizationPart", StringComparison.InvariantCultureIgnoreCase)))) {
                    // At least a HierarchyType is localizable, so we make all TerritoryTypes localizable
                    var territoryTypes = _contentDefinitionManager
                        .ListTypeDefinitions()
                        .Where(ctd => ctd
                            .Parts
                            .Any(pa => pa
                                .PartDefinition
                                .Name
                                .Equals(TerritoryPart.PartName, StringComparison.InvariantCultureIgnoreCase)));
                    foreach (var territoryType in territoryTypes) {
                        _contentDefinitionManager
                            .AlterTypeDefinition(territoryType.Name, builder => builder
                                .WithPart("LocalizationPart"));
                    }
                }
            }
        }

        #region Not used interface methods
        public void Disabled(Feature feature) { }

        public void Disabling(Feature feature) { }

        public void Enabling(Feature feature) { }

        public void Installed(Feature feature) { }

        public void Installing(Feature feature) { }

        public void Uninstalled(Feature feature) { }

        public void Uninstalling(Feature feature) { }
        #endregion
    }
}
