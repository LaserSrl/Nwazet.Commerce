using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using System.Linq;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class CartWeightCriteria : NumericCriterionBase {

        public CartWeightCriteria(
            IProductPriceService productPriceService) {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void Describe(DescribeCriterionContext describe) {
            describe.For("Cart", T("Cart Weight"), T("Cart Weight"))
                .Element("Cart Weight",
                    T("Cart Weight"),
                    T("Cart Weight Criterion"),
                    ApplyCriteria,
                    DisplayCriteria,
                    "NumericFilter");
        }

        public void ApplyCriteria(CriterionContext context) {
            // If the context tells us that already we should not be using
            // the shipping method this criterion is part of, test nothing
            if (context.IsApplicable) {
                // get the comparison func
                var test = Test(context);
                // compute the cart weight
                var weight = context.ApplicabilityContext
                    .ProductQuantities
                    .Sum(pq => pq.Quantity * (decimal)pq.Product.Weight);
                // test
                context.IsApplicable = test(weight);
            }
        }

        public LocalizedString DisplayCriteria(CriterionContext context) {
            return NumericFilterForm.DisplayFilter(T("Cart Weight").Text, context.State, T);
        }
    }
}
