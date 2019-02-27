using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPart
        : ContentPart<FlexibleShippingMethodRecord>, IShippingMethod {
        public string Name {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShippingCompany {
            get { return Retrieve(r => r.ShippingCompany); }
            set { Store(r => r.ShippingCompany, value); }
        }
        public string IncludedShippingAreas {
            get { return Retrieve(r => r.IncludedShippingAreas); }
            set { Store(r => r.IncludedShippingAreas, value); }
        }
        public string ExcludedShippingAreas {
            get { return Retrieve(r => r.ExcludedShippingAreas); }
            set { Store(r => r.ExcludedShippingAreas, value); }
        }

        public decimal DefaultPrice {
            get { return Retrieve(r => r.DefaultPrice); }
            set { Store(r => r.DefaultPrice, value); }
        }

        public IList<ApplicabilityCriterionRecord> ApplicabilityCriteria {
            get { return Record.ApplicabilityCriteria; }
        }

        public IEnumerable<ShippingOption> ComputePrice(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities, 
            IEnumerable<IShippingMethod> shippingMethods, 
            string country, 
            string zipCode, 
            IWorkContextAccessor workContextAccessor) {

            return null;
        }
    }
}
