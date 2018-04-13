using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public VatConfigurationPartHandler(
            IRepository<VatConfigurationPartRecord> repository,
            IContentManager contentManager,
            ISiteService siteService) {

            _contentManager = contentManager;
            _siteService = siteService;

            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<VatConfigurationSiteSettingsPart>("Site"));

            // Lazyfield setters and loaders
            OnInitializing<VatConfigurationPart>(PropertySetHandlers);
            OnLoading<VatConfigurationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<VatConfigurationPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            
            // manage the case where the default configuration is deleted
            OnRemoved<VatConfigurationPart>((context, part) => ResetDefaultVatConfigurationPart(part));
            OnDestroyed<VatConfigurationPart>((context, part) => ResetDefaultVatConfigurationPart(part));
        }

        static void PropertySetHandlers(
            InitializingContentContext context, VatConfigurationPart part) {

            part.HierarchyField.Setter(hierarchy => {
                part.Record.Hierarchy = hierarchy.As<TerritoryHierarchyPart>().Record;
                return hierarchy;
            });

            if (part.HierarchyField.Value != null) {
                part.HierarchyField.Value = part.HierarchyField.Value;
            }
        }

        void LazyLoadHandlers(VatConfigurationPart part) {

            part.HierarchyField.Loader(() => {
                if (part.Record.Hierarchy != null) {
                    return _contentManager
                        .Get<ContentItem>(part.Record.Hierarchy.Id,
                            VersionOptions.Latest, QueryHints.Empty);
                } else {
                    return null;
                }
            });
        }

        void ResetDefaultVatConfigurationPart(VatConfigurationPart part) {
            // We will prevent removing the part that has the default configuration. However
            // here we still manage the case where that part is removed, in order to have a
            // further layer of data consistency. We may end up here if a delete/remove is invoked
            // without going through a permission check.
            var settings = _siteService.GetSiteSettings().As<VatConfigurationSiteSettingsPart>();
            if (settings.DefaultVatConfigurationId == part.ContentItem.Id) {
                settings.DefaultVatConfigurationId = 0;
            }
        }
    }
}
