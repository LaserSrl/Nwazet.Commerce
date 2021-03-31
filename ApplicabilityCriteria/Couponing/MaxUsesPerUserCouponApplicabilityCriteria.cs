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
    public class MaxUsesPerUserCouponApplicabilityCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        // This criterion doesn't exactly require the user to be authenticated, but
        // it needs a way to understand who the user is. It will try to use the
        // CurrentUser: if that is null, it will try to figure out who is using the
        // coupon through the implementations of ICouponUserIdentifierProvider. If
        // it finds no way to identify a user, the test for the coupon will fail.

        private readonly IUsedCouponsRepositoryService _usedCouponsRepositoryService;
        private readonly IEnumerable<ICouponUserIdentifierProvider> _couponUserIdentifierProviders;

        public MaxUsesPerUserCouponApplicabilityCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IUsedCouponsRepositoryService usedCouponsRepositoryService,
            IEnumerable<ICouponUserIdentifierProvider> couponUserIdentifierProviders)
            : base(workContextAccessor, cacheManager, signals) {

            _usedCouponsRepositoryService = usedCouponsRepositoryService;
            _couponUserIdentifierProviders = couponUserIdentifierProviders
                .OrderByDescending(cuip => cuip.Priority);

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string ProviderName =>
            "MaxUsesPerUserCouponApplicabilityCriteria";

        public override LocalizedString ProviderDisplayName =>
            T("Criterion on the number of coupon uses per user.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Number of uses", 
                    T("Number of times the coupon was used"), 
                    T("Number of times the coupon was used"))
                .Element("Number of times the user used this coupon",
                    T("Number of times the user used this coupon"),
                    T("Number of times the user used this coupon"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => T("User has used the coupon fewer than '{0}' times", (int)ctx.State.Value),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    PositiveIntegerValueForm.FormName);
        }

        private void ApplyCriterion(
            CouponApplicabilityCriterionContext context) {

            if (context.IsApplicable) {
                // get the value we should compare things to
                var sValue = (string)context.State.Value;
                var value = PositiveIntegerValueForm.ParseStateValue(sValue);
                if (value <= 0) {
                    // misconfiguration
                    context.ApplicabilityContext.Message =
                        T("Coupon code {0} is not valid", context.CouponRecord.Code);
                    context.IsApplicable = false;
                    context.ApplicabilityContext.IsApplicable = false;
                } else {
                    // value is positive
                    // How many times has the user used this coupon?
                    var userId = context.ApplicabilityContext
                        .WorkContext?.CurrentUser?.Id ?? 0;
                    var pastUses = -1;
                    if (userId <= 0) {
                        // doesn't look like we have an authenticated user, so we 
                        // try to use different providers to identify the number of past
                        // uses for this coupon.
                        foreach (var provider in _couponUserIdentifierProviders) {
                            var identifier = provider
                                .GetAdditionalUserIdentifier(context.ApplicabilityContext);
                            if (!string.IsNullOrWhiteSpace(identifier)) {
                                var providerType = provider
                                    .GetIdentifierType(context.ApplicabilityContext);
                                // see if there are uses for the condition for this provider
                                pastUses = _usedCouponsRepositoryService
                                    .Query()
                                    .Where(cur =>
                                        cur.CouponRecord_Id == context.ApplicabilityContext.Coupon.Id
                                        && cur.WasInvalid == false // only count coupons that were actually spent
                                        && cur.IdentifierType == providerType  
                                        && cur.AdditionalUserIdentifier == identifier)
                                    .Count();
                                if (pastUses > 0) {
                                    // if there's any uses, we are done here
                                    break;
                                }
                            }
                        }
                    } else {
                        pastUses = _usedCouponsRepositoryService
                            .Query()
                            .Where(cur => 
                                cur.CouponRecord_Id == context.ApplicabilityContext.Coupon.Id
                                        && cur.WasInvalid == false // only count coupons that were actually spent
                                && cur.UserPartRecord_Id == userId)
                            .Count();
                    }
                    // did we find anything?
                    if (pastUses < 0) {
                        // we could not identify exactly what we should be querying
                        context.ApplicabilityContext.Message =
                            T("Coupon code {0} is not valid", context.CouponRecord.Code);
                        context.IsApplicable = false;
                        context.ApplicabilityContext.IsApplicable = false;
                    } else {
                        // we can actually check
                        var result = pastUses < value;

                        if (!result) {
                            context.ApplicabilityContext.Message =
                                T("Coupon code {0} has been used already", context.CouponRecord.Code);
                        }
                        context.IsApplicable = result;
                        context.ApplicabilityContext.IsApplicable = result;
                    }
                }
            }
        }
    }
}
