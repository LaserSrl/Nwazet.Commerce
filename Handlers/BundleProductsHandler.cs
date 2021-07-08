using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.Bundles")]
    public class BundleProcuctsHandler : ContentHandler {
        private readonly IRepository<BundleProductsRecord> _repositoryBundleProduct;
        private readonly IRepository<ContentItemVersionRecord> _repositoryContent;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;
        private readonly UrlHelper _url;
        private readonly INotifier _notifier;

        public BundleProcuctsHandler(
            IRepository<BundleProductsRecord> repositoryBundleProduct,
            IRepository<ContentItemVersionRecord> repositoryContent,
            IContentManager contentManager,
            IOrchardServices services,
            UrlHelper url,
            INotifier notifier) {

            _repositoryBundleProduct = repositoryBundleProduct;
            _repositoryContent = repositoryContent;
            _contentManager = contentManager;
            _services = services;
            _url = url;
            _notifier = notifier;

            T = NullLocalizer.Instance;

            OnDestroyed<ProductPart>((context, part) => NotifierBundleParent(part));
            OnRemoved<ProductPart>((ctx, part) => NotifierBundleParent(part));
            OnUnpublished<ProductPart>((ctx, part) => NotifierBundleParent(part));
        }

        public Localizer T { get; set; }

        // number of bundles to display, so as not to have a very long list of bundles in the warning
        private int numberBundleDisplayed = 3;

        public void NotifierBundleParent(ProductPart part) {
            // notifier if product is in the bundle

            // select*
            // from Nwazet_Commerce_BundleProductsRecord bp
            // inner join
            // Orchard_Framework_ContentItemVersionRecord civ
            // on bp.BundlePartRecord_Id = civ.ContentItemRecord_id
            // where Published = 1 and bp.ContentItemRecord_Id = 561
            var bundleProductRecord = _repositoryBundleProduct
                .Table
                .Where(b => b.ContentItemRecord.Id == part.Id)
                .Join( // Join A on B=C
                    _repositoryContent.Table,////A
                    rbp => rbp.BundlePartRecord.Id,//B
                    rc => rc.ContentItemRecord.Id,//C
                    (rbp, c) => new { pub = c.Published, b = rbp }
                )
                .Where(x => x.pub)
                .Select(x => x.b)
                .ToList();

            if (bundleProductRecord.Count() > 0) {
                if (bundleProductRecord.Count() <= numberBundleDisplayed) {
                    _notifier.Warning(T("The product {0} is part of: {1}",
                        _contentManager.GetItemMetadata(part.ContentItem).DisplayText,
                        T(string.Join(", ", bundleProductRecord
                            .Select(bpr => {
                                var ci = _contentManager.Get(bpr.BundlePartRecord.Id);
                                if (ci != null) {
                                    return string.Format("<a href=\"{0}\">{1}</a>", _url.ItemEditUrl(ci), _contentManager.GetItemMetadata(ci).DisplayText);
                                }
                                return null;
                            })
                            .Where(t => t != null)))));
                }
                else {
                    _notifier.Warning(T("The product {0} is present in some bundles", _contentManager.GetItemMetadata(part.ContentItem).DisplayText));
                }
            }
        }
    }
}
