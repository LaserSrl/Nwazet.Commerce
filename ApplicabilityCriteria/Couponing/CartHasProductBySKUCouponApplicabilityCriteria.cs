using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CartHasProductBySKUCouponApplicabilityCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        public CartHasProductBySKUCouponApplicabilityCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals)
            : base(workContextAccessor, cacheManager, signals) {

        }

        public override string ProviderName => 
            "CartHasProductBySKUCouponApplicabilityCriteria";

        public override LocalizedString ProviderDisplayName => 
            T("Criteria for whether the cart contains specific products based on their SKU.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                .Element("Product SKUs match provided ones",
                    T("Product SKUs match provided ones"),
                    T("Product SKUs match provided ones"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => CouponProductSkuFilterForm.DisplayFilter(T, ctx.State),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    CouponProductSkuFilterForm.FormName);
        }

        private void ApplyCriterion(
            CouponApplicabilityCriterionContext context) {

            if (context.IsApplicable) {
                // skus from the cart:
                var cartSkus = context.ApplicabilityContext
                    .ShoppingCart
                    .GetProducts()
                    .Select(scqp => scqp.Product.Sku);
                // do configured test
                var result = CouponProductSkuFilterForm.EvaluateFilter(cartSkus, context.State);

                if (!result) {
                    context.ApplicabilityContext.Message =
                        T("Coupon code {0} is not valid", context.CouponRecord.Code);
                }
                context.IsApplicable = result;
                context.ApplicabilityContext.IsApplicable = result;
            }
        }
    }
}
