using System.Collections.Generic;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.ViewModels {
    public class BundleViewModel {
        public IList<ProductEntry> Products { get; set; }
        public BundlePart BundlePart { get; set; }
    }

    public class ProductEntry {
        public int ProductId { get; set; }
        public IContent Product { get; set; }
        public int Quantity { get; set; }
        public string DisplayText { get; set; }
    }
    public class ProductEntryAutocomplete {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string EditUrl { get; set; }
        public string DisplayText { get; set; }
        public string Sku { get; set; }
        public string Lang { get; set; }
        public string DifferentLang { get; set; }
        public bool HasPublished { get; set; }
    }
}
