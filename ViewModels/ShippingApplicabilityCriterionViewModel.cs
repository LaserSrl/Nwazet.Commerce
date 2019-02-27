using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    public class ShippingApplicabilityCriterionViewModel {

        public ShippingApplicabilityCriterionViewModel() {

        }

        public ShippingApplicabilityCriterionViewModel(
            ApplicabilityCriterionRecord record) {

            RecordId = record.Id;
            Type = record.Type;
            DisplayText = string.IsNullOrWhiteSpace(record.Description)
                ? "" // TODO
                : record.Description;
        }

        public int RecordId { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
    }
}
