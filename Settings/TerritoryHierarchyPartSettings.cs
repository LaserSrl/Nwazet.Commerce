using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Settings {
    [OrchardFeature("Territories")]
    public class TerritoryHierarchyPartSettings {
        [Required]
        public string TerritoryType { get; set; }
        public bool MayChangeTerritoryTypeOnItem { get; set; }
    }
}
