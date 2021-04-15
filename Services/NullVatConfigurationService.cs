using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Services {
    public class NullVatConfigurationService : IVatConfigurationService {
        // Null service implementation to prevent dependency injection issues if the 
        // AdvancedVAT feature is not active.
        public VatConfigurationPart GetDefaultCategory() {
            return null;
        }

        public int GetDefaultCategoryId() {
            return 0;
        }

        public TerritoryInternalRecord GetDefaultDestination() {
            return null;
        }

        public decimal GetRate(ProductPart part) {
            return 0.0m;
        }

        public decimal GetRate(ProductPart part, TerritoryInternalRecord destination) {
            return 0.0m;
        }

        public decimal GetRate(ProductPart part, IEnumerable<TerritoryInternalRecord> destination) {
            return 0.0m;
        }

        public decimal GetRate(ProductPart part, string country, string zipcode) {
            return 0.0m;
        }

        public decimal GetRate(VatConfigurationPart vatConfig) {
            return 0.0m;
        }

        public decimal GetRate(VatConfigurationPart vatConfig, TerritoryInternalRecord destination) {
            return 0.0m;
        }

        public decimal GetRate(VatConfigurationPart vatConfig, IEnumerable<TerritoryInternalRecord> destination) {
            return 0.0m;
        }


        public void SetDefaultCategory(VatConfigurationPart part) {
            // do nothing
        }
    }
}
