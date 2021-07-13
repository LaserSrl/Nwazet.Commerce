using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Linq;

namespace Nwazet.Commerce.ApplicabilityCriteria.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class MinCartTotalCouponApplicabilityCriterion
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        public MinCartTotalCouponApplicabilityCriterion(
           IWorkContextAccessor workContextAccessor,
           ICacheManager cacheManager,
           ISignals signals)
           : base(workContextAccessor, cacheManager, signals) {

        }
        public override string ProviderName =>
            "MinimunPriceCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName =>
            T("Criterion on the total price of cart.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            var symbolCurrecy = GetCurrency();

            describe
                .For("Cart", T("Total Cart"), T("Total Cart"))
                .Element("The cart total must be at least",
                    T("The cart total must be at least {0}", symbolCurrecy),
                    T("The cart total must be at least {0}", symbolCurrecy),
                    (ctx) => PostCanBeAdded(ctx),
                    (ctx) => PostCanBeProcessed(ctx),
                    (ctx) => T("The cart total must be at least {0} {1}",
                        symbolCurrecy,
                        (decimal)ctx.State.Value),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    MinCartTotalForm.FormName);
        }

        private string GetCurrency() {
            var currencyCode = _workContextAccessor.GetContext()
                .CurrentSite.As<ECommerceCurrencySiteSettingsPart>().CurrencyCode;

            var currency = Currency.Currencies[currencyCode];
            return currency.Symbol;
        }

        private void PostCanBeAdded(CouponPostApplicabilityContext context) {
            if (context.IsApplicable) {
                Evaluate(context);
            }
        }

        private void PostCanBeProcessed(CouponPostApplicabilityContext context) {
            if (context.IsApplicable) {
                Evaluate(context);
            }
        }

        private void Evaluate(
            CouponPostApplicabilityContext context) {

            var minSubtotal = (decimal)context.State.Value;
            context.IsApplicable &= (context.BaseCartSubtotal
                // Values are negative already
                + context.CouponValues
                    .Where(c=>c.Coupon!=context.Coupon)
                    .Sum(cv => cv.Value) > minSubtotal);
        }
    }
}
