using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPartDriver : ContentPartDriver<TerritoryHierarchyPart> {

        public TerritoryHierarchyPartDriver() { }

        protected override DriverResult Display(TerritoryHierarchyPart part, string displayType, [Dynamic] dynamic shapeHelper) {
            return base.Display(part, displayType, shapeHelper);
        }

        protected override DriverResult Editor(TerritoryHierarchyPart part, [Dynamic] dynamic shapeHelper) {
            return base.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(TerritoryHierarchyPart part, IUpdateModel updater, [Dynamic] dynamic shapeHelper) {
            return base.Editor(part, updater, shapeHelper);
        }
    }
}
