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
        
        public override decimal GetDiscountPrice(ProductPart part) {
            return part.DiscountPrice + part.DiscountPrice * GetRate(part);
        }

        public override decimal GetPrice(ProductPart part) {
            return part.Price + part.Price * GetRate(part);
        }

        public override decimal GetPrice(ProductPart part, decimal basePrice) {
            return basePrice + basePrice * GetRate(part);
        }
        
        private decimal GetRate(ProductPart part) {
            return _vatConfigurationService.GetRate(part);
        }
    }
}
