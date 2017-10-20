using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories.Handlers {
    public class TerritoryMockHandler : ContentHandler {
        public TerritoryMockHandler() {
            Filters.Add(new ActivatingFilter<TerritoryPart>("TerritoryType0"));
            Filters.Add(new ActivatingFilter<TerritoryPart>("TerritoryType1"));
            Filters.Add(new ActivatingFilter<TerritoryPart>("TerritoryType2"));
        }
    }
}
