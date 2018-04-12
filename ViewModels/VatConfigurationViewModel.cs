using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatConfigurationViewModel {

        public VatConfigurationViewModel() {
            Hierarchies = new List<SelectListItem>();
        }

        [Required]
        public string TaxProductCategory { get; set; }
        public bool IsDefaultCategory { get; set; }
        public decimal DefaultRate { get; set; }
        public int Priority { get; set; }

        [Required]
        public int SelectedHierarchyId { get; set; }

        public string SelectedHierarchyText { get; set; }
        public ContentItem SelectedHierarchyItem { get; set; }

        public IEnumerable<SelectListItem> Hierarchies { get; set; }

        public VatConfigurationPart Part { get; set; }
    }
}
