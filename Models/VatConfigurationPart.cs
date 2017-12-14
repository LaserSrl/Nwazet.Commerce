using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPart : ContentPart<VatConfigurationPartRecord>, ITax {
        public string Name {
            get { return Retrieve(r => r.Category); }
            set { Store(r => r.Category, value); }
        }

        public int Priority {
            get { return Retrieve(r => r.Priority); }
            set { Store(r => r.Priority, value); }
        }
        
        public decimal ComputeTax(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities, 
            decimal subtotal, decimal shippingCost, string country, string zipCode) {

            // For this method the cart's subtotal does not matter and will always be ignored.
            
            // Since we cannot inject services here, the part will need to have access to its own
            // table describing the relationships between product categories and destinations.

            decimal taxTotal = 0;

            foreach (var productQuantity in productQuantities) {
                // get the category for this line of products
                // given category and destination (country + zipcode)
                
            }


            return taxTotal;
        }

        public string Category {
            get { return Retrieve(r => r.Category); }
            set { Store(r => r.Category, value); }
        }

        public bool IsDefaultCategory {
            get { return Retrieve(r => r.IsDefaultCategory); }
            set { Store(r => r.IsDefaultCategory, value); }
        }

        public decimal DefaultRate {
            get { return Retrieve(r => r.DefaultRate); }
            set { Store(r => r.DefaultRate, value); }
        }

        private readonly LazyField<ContentItem> _hierarchy =
            new LazyField<ContentItem>();

        public LazyField<ContentItem> HierarchyField {
            get { return _hierarchy; }
        }

        public ContentItem Hierarchy {
            get { return _hierarchy.Value; }
        }

        public TerritoryHierarchyPart HierarchyPart {
            get { return Hierarchy?.As<TerritoryHierarchyPart>(); }
        }
    }
}
