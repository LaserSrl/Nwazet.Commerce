using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryInternalRecord {
        public virtual int Id { get; set; } //Primary Key
        public virtual string Name { get; set; } //Name given to the territory
    }
}
