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

        protected override string Prefix {
            get { return "TerritoryHierarchyPart"; }
        }

        protected override DriverResult Display(TerritoryHierarchyPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_TerritoryHierarchy_SummaryAdmin",
                    () => shapeHelper.Parts_TerritoryHierarchy_SummaryAdmin(part)
                    ));
        }

        protected override DriverResult Editor(TerritoryHierarchyPart part, dynamic shapeHelper) {
            //TODO
            return null;
        }

        protected override DriverResult Editor(TerritoryHierarchyPart part, IUpdateModel updater, dynamic shapeHelper) {
            //TODO
            return null;
        }
    }
}
