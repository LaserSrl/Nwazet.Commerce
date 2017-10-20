using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories.Handlers {
    public class TerritoryHierachyMockHandler : ContentHandler {
        public TerritoryHierachyMockHandler() {
            Filters.Add(new ActivatingFilter<TerritoryHierarchyPart>("HierarchyType0"));
            Filters.Add(new ActivatingFilter<TerritoryHierarchyPart>("HierarchyType1"));
            Filters.Add(new ActivatingFilter<TerritoryHierarchyPart>("HierarchyType2"));
        }
    }
}
