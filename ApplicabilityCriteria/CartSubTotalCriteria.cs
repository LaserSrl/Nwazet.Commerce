using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CartSubTotalCriteria : IApplicabilityCriterionProvider {

        public CartSubTotalCriteria() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeCriterionContext describe) {

            describe.For("Cart", T("Cart Subtotal"), T("Cart Subtotal"))
                .Element("Cart Subtotal",
                    T("Cart Subtotal"),
                    T("Cart Subtotal Criterion"),
                    ApplyCriteria,
                    DisplayCriteria,
                    "NumericFilter");
        }

        public void ApplyCriteria(CriterionContext context) { }

        public LocalizedString DisplayCriteria(CriterionContext context) {
            return NumericFilterForm.DisplayFilter(T("Cart Subtotal").Text, context.State, T);
        }

    }
}
