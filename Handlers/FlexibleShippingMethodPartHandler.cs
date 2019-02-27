using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPartHandler : ContentHandler {

        public FlexibleShippingMethodPartHandler(
            IRepository<FlexibleShippingMethodRecord> repository) {

            Filters.Add(StorageFilter.For(repository));
        }
    }
}
