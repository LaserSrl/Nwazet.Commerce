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
    public class HierarchyVatConfigurationPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public HierarchyVatConfigurationPartHandler(
            IRepository<HierarchyVatConfigurationPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
        }

        static void PropertySetHandlers(
            InitializingContentContext context, HierarchyVatConfigurationPart part) {

            //part.VatConfigurationsField.Setter(value => {
            //    return value
            //        .Where(vcp => vcp
            //            .Record
            //            .HierarchyConfigurationIntersections
            //            .Any(hci => hci.Hierarchy == part.Record))
            //        .ToList();
            //});

            // call the setter in case a value had already been set
            if (part.VatConfigurationsField.Value != null) {
                part.VatConfigurationsField.Value = part.VatConfigurationsField.Value;
            }
        }

        void LazyLoadHandlers(HierarchyVatConfigurationPart part) {

            //part.VatConfigurationsField.Loader(() => {
            //    if (part.Record.VatConfigurationIntersections != null
            //        && part.Record.VatConfigurationIntersections.Any()) {
            //        return _contentManager
            //            .GetMany<VatConfigurationPart>(part.Record.VatConfigurationIntersections
            //                .Select(vci => vci.VatConfiguration.Id),
            //                VersionOptions.Latest, QueryHints.Empty);
            //    } else {
            //        return Enumerable.Empty<VatConfigurationPart>();
            //    }
            //});
        }
    }
}
