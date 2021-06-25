using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Settings;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Mvc.Html;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Bundles")]
    public class BundleAutocompleteService : BundleAutocompleteServiceBase {

        public BundleAutocompleteService(
            IContentManager contentManager,
            UrlHelper url)
            : base(contentManager, url) { }


    }
}