using Nwazet.Commerce.Descriptors.CouponApplicability;
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
    public class AuthenticationStateCouponApplicabilityCriterion 
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        public AuthenticationStateCouponApplicabilityCriterion(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals)
            : base(workContextAccessor, cacheManager, signals) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string ProviderName => "AuthenticationStateCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName => T("Criteria on the authentication state");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();
            describe
                .For("User", T("Authentication State"), T("Authentication State"))
                .Element("User is authenticated",
                    T("User is authenticated"),
                    T("User is authenticated"),
                    (ctx) => ApplyCriteria(ctx, (b) => b, T("Coupon {0} is only available to authenticated users.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriteria(ctx, (b) => b, T("Coupon {0} is only available to authenticated users.", ctx.CouponRecord.Code)),
                    (ctx) => T("User is authenticated"),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null) // null form because there's nothing special to configure
                .Element("User is not authenticated",
                    T("User is not authenticated"),
                    T("User is not authenticated"),
                    (ctx) => ApplyCriteria(ctx, (b) => !b, T("Coupon {0} is not available to authenticated users.", ctx.CouponRecord.Code)),
                    (ctx) => ApplyCriteria(ctx, (b) => !b, T("Coupon {0} is not available to authenticated users.", ctx.CouponRecord.Code)),
                    (ctx) => T("User is not authenticated"),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    null);
        }
        
        public void ApplyCriteria(CouponApplicabilityCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // true/false
            Func<bool, bool> outerCriterion,
            LocalizedString failureMessage) {

            if (context.IsApplicable) {
                var result = outerCriterion(
                    context?.ApplicabilityContext?.WorkContext?.CurrentUser != null);
                if (!result) {
                    context.ApplicabilityContext.Message = failureMessage;
                }
                context.IsApplicable = result;
                context.ApplicabilityContext.IsApplicable = result;
            }
        }
    }
}
