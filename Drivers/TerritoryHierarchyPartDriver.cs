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
            var shapes = new List<DriverResult>();
            //part.Id == 0: new item
            if (part.Id != 0) {
                //TODO: if the part is fully configured
                //add a shape that allows managing the territories in the hierarchy
            }
            //add a shape for configuration of the part
            //Some configuration options may be locked depending on the territories in the hierarchy
            //TODO
            return Combined(shapes.ToArray());
        }

        protected override DriverResult Editor(TerritoryHierarchyPart part, IUpdateModel updater, dynamic shapeHelper) {
            //TODO
            return null;
        }
    }
}
