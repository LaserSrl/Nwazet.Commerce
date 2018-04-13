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

        #region ITax implementation
        public string Name {
            get { return TaxProductCategory; }
            set { TaxProductCategory = value; }
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
        #endregion

        /// <summary>
        /// This would be the product category this VAT will apply to. Uniqueness of this will have to be
        /// enforced in code, because the migrations fail if we attempt to set this (actually, the corresponding
        /// value in the record) as unique.
        /// </summary>
        public string TaxProductCategory {
            get { return Retrieve(r => r.TaxProductCategory) ?? string.Empty; }
            set { Store(r => r.TaxProductCategory, value ?? string.Empty); }
        }
        
        /// <summary>
        /// Default rate for territories in the hierarchy
        /// </summary>
        public decimal DefaultRate {
            get { return Retrieve(r => r.DefaultRate); }
            set { Store(r => r.DefaultRate, value); }
        }

        /// <summary>
        /// Default rate for territories outside the hierarchy
        /// </summary>
        public decimal DefaultExtraRate {
            get { return Retrieve(r => r.DefaultExtraRate); }
            set { Store(r => r.DefaultExtraRate, value); }
        }

        private readonly LazyField<ContentItem> _hierarchy =
            new LazyField<ContentItem>();

        public LazyField<ContentItem> HierarchyField {
            get { return _hierarchy; }
        }

        public ContentItem Hierarchy {
            get { return _hierarchy.Value; }
            set { _hierarchy.Value = value; }
        }

        public TerritoryHierarchyPart HierarchyPart {
            get { return Hierarchy?.As<TerritoryHierarchyPart>(); }
        }
    }
}
