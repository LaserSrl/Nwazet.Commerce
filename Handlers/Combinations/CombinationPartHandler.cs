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

namespace Nwazet.Commerce.Handlers.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class CombinationPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;

        public CombinationPartHandler(
            IRepository<CombinationPartRecord> repository,
            IContentManager contentManager) {

            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));

            //Lazyfield setters
            OnInitializing<CombinationPart>(PropertySetHandlers);
            OnLoading<CombinationPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<CombinationPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));

        }

        static void PropertySetHandlers(
            InitializingContentContext context, CombinationPart part) {

            part.CombinationContainerPartField.Setter(container => {
                part.Record.CombinationContainerPartRecord = 
                    container.As<CombinationContainerPart>().Record;
                return container;
            });

            //call the setters in case a value had already been set
            if (part.CombinationContainerPartField.Value != null) {
                part.CombinationContainerPartField.Value = part.CombinationContainerPartField.Value;
            }
        }

        void LazyLoadHandlers(CombinationPart part) {
            part.CombinationContainerPartField.Loader(() => {
                if (part.Record.CombinationContainerPartRecord != null) {
                    return _contentManager
                        .Get<CombinationContainerPart>(part.Record.CombinationContainerPartRecord.Id,
                            VersionOptions.Latest, QueryHints.Empty);
                } else {
                    return null;
                }

            });
        }
    }
}
