using Orchard.ContentManagement;
using Orchard.Core.Contents.ViewModels;
using System.Collections.Generic;

namespace Nwazet.Commerce.ViewModels {
    public class ListProductsViewModel {
        public ListProductsViewModel() {
            Options = new ContentProductOptions();
        }

        public int? Page { get; set; }
        public IList<Entry> Entries { get; set; }
        public ContentProductOptions Options { get; set; }

        public string Id { get; set; }

        public string TypeName {
            get { return Id; }
        }
        public string TypeDisplayName { get; set; }

        public class Entry {
            public ContentItem ContentItem { get; set; }
            public ContentItemMetadata ContentItemMetadata { get; set; }
        }
    }

    public class ContentProductOptions {
        public ContentProductOptions() {
            OrderBy = ContentsProduct.Modified;
            ContentsStatus = ContentsStatus.Latest;
            FilterDiscount = DiscountProduct.ShowAll;
        }

        public string Title { get; set; }
        public string Sku { get; set; }
        public string SelectedCulture { get; set; }
        public string SelectedFilter { get; set; }
        public string PriceFrom { get; set; }
        public string PriceTo { get; set; }
        public string InventoryFrom { get; set; }
        public string InventoryTo { get; set; }

        public DiscountProduct FilterDiscount { get; set; }
        public ContentsStatus ContentsStatus { get; set; }
        public IEnumerable<KeyValuePair<string, string>> FilterOptions { get; set; }
        public ContentsProduct OrderBy { get; set; }
        public IEnumerable<string> Cultures { get; set; }
    }

    public enum ContentsProduct {
        Modified,
        Published,
        Created
    }

    public enum DiscountProduct {
        Discount,
        NoDiscount,
        ShowAll
    }
}
