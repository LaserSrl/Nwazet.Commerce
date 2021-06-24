using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Bundles")]
    public abstract class BundleAutocompleteServiceBase : IBundleAutocompleteService {
        public virtual BundleViewModel BuildEditorViewModel(BundlePart part) {
            return new BundleViewModel();
        }

        public virtual List<ProductEntryAutocomplete> GetProducts(int contentItemId,string searchtext, List<int> excludedProductIds) {
            return new List<ProductEntryAutocomplete>();
        }
    }
}
