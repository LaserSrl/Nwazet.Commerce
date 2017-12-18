using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public VatConfigurationPartHandler(
            IRepository<VatConfigurationPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<VatConfigurationSiteSettingsPart>("Site"));

            //Lazyfield setters and loaders
            OnInitializing<VatConfigurationPart>(PropertySetHandlers);
            OnLoading<VatConfigurationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<VatConfigurationPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

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

    }
}
