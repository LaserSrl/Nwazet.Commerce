using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Nwazet.Commerce.ViewModels {
    public class ListProductsViewModel {
        public ListProductsViewModel() {
            Options = new ContentProductOptions();
        }

        public int? Page { get; set; }
        public IList<Entry> Entries { get; set; }
        public ContentProductOptions Options { get; set; }

        public class Entry {
            public ContentItem ContentItem { get; set; }
            public ContentItemMetadata ContentItemMetadata { get; set; }
        }
    }

    public class ContentProductOptions {
        public ContentProductOptions() {
            OrderBy = ContentsProduct.Modified;
        }

        public string Title { get; set; }
        public string Sku { get; set; }
        public string SelectedFilter { get; set; }
        public IEnumerable<KeyValuePair<string, string>> FilterOptions { get; set; }
        public ContentsProduct OrderBy { get; set; }
    }

    public enum ContentsProduct {
        Modified,
        Published,
        Created
    }
}
