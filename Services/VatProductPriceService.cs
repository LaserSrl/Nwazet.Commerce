using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatProductPriceService : BaseProductPriceService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly ITerritoriesService _territoriesService;

        public VatProductPriceService(
            IWorkContextAccessor workContextAccessor,
            IVatConfigurationService vatConfigurationService,
            ITerritoriesRepositoryService territoriesRepositoryService,
            ITerritoriesService territoriesService) {

            _workContextAccessor = workContextAccessor;
            _vatConfigurationService = vatConfigurationService;
            _territoriesRepositoryService = territoriesRepositoryService;
            _territoriesService = territoriesService;
        }

        private VatConfigurationSiteSettingsPart _settings { get; set; }

        private VatConfigurationSiteSettingsPart Settings {
            get {
                if (_settings == null) {
                    _settings = _workContextAccessor.GetContext().CurrentSite.As<VatConfigurationSiteSettingsPart>();
                }
                return _settings;
            }
        }

        public override decimal GetDiscountPrice(ProductPart part) {
            return part.DiscountPrice + part.DiscountPrice * GetRate(part);
        }

        public override decimal GetPrice(ProductPart part) {
            return part.Price + part.Price * GetRate(part);
        }

        private VatConfigurationPart GetVatConfiguration(ProductPart part) {
            return part
                ?.As<ProductVatConfigurationPart>()
                ?.VatConfigurationPart
                ?? _vatConfigurationService.GetDefaultCategory();
        }

        private decimal GetRate(ProductPart part) {
            // Given the VatConfiguration for this product, 
            // find the rate for the front end
            if (Settings.DefaultTerritoryForVatId == 0) {
                // Do not add tax for front end, i.e. the price shown on front end is "before tax"
                return 0;
            }

            var defaultTerritory = _territoriesRepositoryService
                .GetTerritoryInternal(Settings.DefaultTerritoryForVatId);

            var vatConfig = GetVatConfiguration(part);

            if (vatConfig == null) {
                // No vat configuration exists
                return 0;
            }

            var hierarchyConfigs = vatConfig
                .Hierarchies
                ?.Where(tup => {
                    var thp = tup.Item1;
                    return thp
                        .Record
                        .Territories
                        .Any(tpr => tpr.TerritoryInternalRecord != null
                            && tpr.TerritoryInternalRecord.Id == defaultTerritory.Id);
                });

            if (hierarchyConfigs == null || !hierarchyConfigs.Any()) {
                // territory is not in the hierarchies, so use the default rate
                return vatConfig.DefaultRate;
            }

            // get the territory exception if it exists
            var territoryConfig = vatConfig
                .Territories
                ?.Where(tup => {
                    var tp = tup.Item1;
                    return tp
                        .Record
                        .TerritoryInternalRecord
                        .Id == defaultTerritory.Id;
                });
            if (territoryConfig == null || !territoryConfig.Any()) {
                // see if the default territory is a child of a territory with a configured
                // rate
                territoryConfig = vatConfig
                    .Territories
                    ?.Where(tup => {
                        var tp = tup.Item1;
                        var children = tp.Children;
                        var isChild = false;
                        while (children != null && children.Any()) {
                            isChild = children // search through the children
                                .Any(ci => {
                                    var territory = ci.As<TerritoryPart>();
                                    return territory != null //sanity check
                                        && territory.Record.TerritoryInternalRecord.Id == defaultTerritory.Id;
                                });
                            if (isChild) {
                                break;
                            }
                            // then we search through the children's children
                            children = children
                                .Where(ci => ci.As<TerritoryPart>() != null) //sanity chedk
                                .SelectMany(ci => ci.As<TerritoryPart>().Children);
                        }
                        return isChild;
                    });
            }

            if (territoryConfig == null || !territoryConfig.Any()) {
                // there is no territory-specific configuration

                // We handle the error case where we have multiple territories satisfying the query by
                // sending the minimum of the rates. If there is only a single configuration for hierarchies
                // (the correct case) the following instruction will return the only rate.
                return hierarchyConfigs.Select(tup => tup.Item2).Min();
            }

            // We handle the error case where we have multiple territories satisfying the query by
            // sending the minimum of the rates. If there is only a single configuration for territories
            // (the correct case) the following instruction will return the only rate.
            return territoryConfig.Select(tup => tup.Item2).Min();

            // the way this method is written, having a configuration specific for a territory "fixes" the 
            // error condition where the territory is ni more than one hierarchy
        }
    }
}
