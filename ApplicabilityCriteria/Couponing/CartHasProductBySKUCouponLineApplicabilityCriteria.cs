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
    public class CartHasProductBySKUCouponLineApplicabilityCriteria
        : BaseCouponCriterionProvider, ICouponLineApplicabilityCriterionProvider {


        public CartHasProductBySKUCouponLineApplicabilityCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals)
            : base(workContextAccessor, cacheManager, signals) {

        }

        public override string ProviderName =>
            "CartHasProductBySKUCouponLineApplicabilityCriteria";

        public override LocalizedString ProviderDisplayName =>
            T("Coupon applies only to items with specific SKUs.");

        public void Describe(DescribeCouponLineApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Cart", T("Cart products"), T("Cart products"))
                .Element("Line product's SKU matches one from a list",
                    T("Line product's SKU matches one from a list"),
                    T("Line product's SKU matches one from a list"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => CouponSingleProductSkuFilterForm.DisplayFilter(T, ctx.State),
                    (ctx) => T("Coupon {0} is not valid for any product in your cart.", ctx.Coupon.Code),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    CouponSingleProductSkuFilterForm.FormName);
        }

        private void ApplyCriterion(CouponLineCriterionContext context) {
            if (context.IsApplicable) {

                var product = context.ApplicabilityContext
                    .CartLine
                    ?.Product;
                var sku = product != null ? product.Sku : string.Empty;

                var result = !string.IsNullOrWhiteSpace(sku);
                if (result) {
                    result = CouponSingleProductSkuFilterForm.EvaluateFilter(sku, context.State);
                }

                if (!result) {
                    var productName = "";
                    var contentManager = product?.ContentItem?.ContentManager;
                    if (contentManager != null) {
                        productName = contentManager.GetItemMetadata(product).DisplayText;
                    }
                    if (!string.IsNullOrWhiteSpace(productName)) {
                        context.ApplicabilityContext.Message =
                            T("Coupon {0} is not valid for {1}.",
                                context.CouponRecord.Code,
                                productName);
                    } else {
                        context.ApplicabilityContext.Message =
                            T("Coupon {0} is not valid.",
                                context.CouponRecord.Code);
                    }
                }

                context.ApplicabilityContext.IsApplicable = result;
                context.IsApplicable = result;
            }
        }
    }
}
