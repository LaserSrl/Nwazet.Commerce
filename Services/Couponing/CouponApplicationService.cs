using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponApplicationService : 
        ICouponApplicationService {

        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INotifier _notifier;
        private readonly IUsedCouponsRepositoryService _usedCouponsRepositoryService;
        private readonly IEnumerable<ICouponUserIdentifierProvider> _couponUserIdentifierProviders;
        private readonly ICouponEvaluationService _couponEvaluationService;

        public CouponApplicationService(
            ICouponRepositoryService couponRepositoryService,
            IWorkContextAccessor workContextAccessor,
            INotifier notifier,
            IUsedCouponsRepositoryService usedCouponsRepositoryService,
            IEnumerable<ICouponUserIdentifierProvider> couponUserIdentifierProviders,
            ICouponEvaluationService couponEvaluationService) {

            _couponRepositoryService = couponRepositoryService;
            _workContextAccessor = workContextAccessor;
            _notifier = notifier;
            _usedCouponsRepositoryService = usedCouponsRepositoryService;
            _couponUserIdentifierProviders = couponUserIdentifierProviders
                .OrderByDescending(cuip => cuip.Priority);
            _couponEvaluationService = couponEvaluationService;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
            _notificationsSent = new HashSet<string>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
                
        #region Coupon lifecycle

        public void ApplyCoupon(CouponApplicabilityContext context) {
            // given the code, find the coupon
            var coupon = GetCouponFromCode(context.CouponCode);
            if (coupon != null) {
                // given the coupon, check whether it's usable
                // then check whether it applies to the current "transaction"
                context.Coupon = coupon;
                context.IsApplicable = coupon.Published;

                if (_couponEvaluationService.CanApply(context)) {

                    Apply(context);
                    _notifier.Information(T("Coupon {0} was successfully applied", context.Coupon.Code));
                }
            } else {

                if (context.Message == null || string.IsNullOrWhiteSpace(context.Message.Text)) {
                    context.Message = T("Coupon code {0} is not valid", context.Coupon?.Code ?? context.CouponCode);
                }

                Warning(context.Message);
            }
        }

        public void RemoveCoupon(CouponApplicabilityContext context) {
            if (context.Coupon == null) {
                context.Coupon = GetCouponFromCode(context.CouponCode);
            }
            if (RemoveCouponInternal(context)) {
                _notifier.Information(T("Coupon {0} was removed", context.CouponCode));
                // formal step for coherence with the rest of the API
                context.IsApplicable = true;
            } else {
                // formal step for coherence with the rest of the API
                context.IsApplicable = false;
                context.Message = T("Coupon code {0} is not valid", context.Coupon?.Code ?? context.CouponCode);
            }
        }

        public void ReevaluateValidity(CouponLifeUpdateContext context) {
            // TODO add to context a flag telling us whether we should remove
            // coupons that can't be processed anymore

            var applicabilityContext = new CouponApplicabilityContext {
                Coupon = context.Coupon,
                ShoppingCart = context.ShoppingCart,
                WorkContext = context.WorkContext,
                IsApplicable = context.Coupon.Published
            };
            if (!_couponEvaluationService.CanProcess(applicabilityContext)) {
                // if the coupon is not valid anymore for the current cart,
                // should we remove it?
                // TODO: for now we choose to not remove it.
                //if (RemoveCouponInternal(applicabilityContext)) {
                //    // TODO: should this message be different?
                //    _notifier.Information(T("Coupon {0} was removed", context.Coupon.Code));
                //}
            }
        }

        public void CouponUsed(CouponUsedContext context) {
            //TODO
            // Maybe it would make sense to fire off coupon-related events?

            // based on the information in the context, create a new CouponUsedRecord.
            if (context != null && context.Coupon != null) {
                var couponUsedRecord = new CouponUsedRecord();
                couponUsedRecord.CouponRecord_Id = context.Coupon?.Id ?? 0;
                couponUsedRecord.UserPartRecord_Id = context.WorkContext?.CurrentUser?.Id ?? 0;
                if (context.Order != null) {
                    couponUsedRecord.OrderPartRecord_Id = context.Order.Record?.Id ?? 0;
                    var commonPart = context.Order.As<CommonPart>();
                    if (commonPart != null && commonPart.CreatedUtc.HasValue) {
                        couponUsedRecord.DateTimeUTC = commonPart.CreatedUtc.Value;
                    } else {
                        couponUsedRecord.DateTimeUTC = DateTime.UtcNow;
                    }
                } else {
                    couponUsedRecord.OrderPartRecord_Id = 0;
                    couponUsedRecord.DateTimeUTC = DateTime.UtcNow;
                }
                // Test whether the coupon could be used:
                var applicabilityContext = new CouponApplicabilityContext {
                    Coupon = context.Coupon,
                    ShoppingCart = context.ShoppingCart,
                    WorkContext = context.WorkContext,
                    IsApplicable = context.Coupon.Published
                };
                couponUsedRecord.WasInvalid = !_couponEvaluationService.CanProcess(applicabilityContext);

                if (_couponUserIdentifierProviders.Any()) {
                    // TODO: providers to set the values of
                    // couponUsedRecord.AdditionalUserIdentifier
                    // and
                    // couponUsedRecord.IdentifierType
                    // The providers should be put in a property of context so they can be passed
                    // around to whatever code needs them.
                    foreach (var provider in _couponUserIdentifierProviders) {
                        var identifier = provider.GetAdditionalUserIdentifier(applicabilityContext);
                        if (!string.IsNullOrWhiteSpace(identifier)) {
                            couponUsedRecord.AdditionalUserIdentifier = identifier;
                            couponUsedRecord.IdentifierType = provider.GetIdentifierType(applicabilityContext);
                            // found the highest priority thing, so we are done?
                            break;
                        }
                    }
                }

                // that CouponUsedRecord should actually be saved in the db
                _usedCouponsRepositoryService.CreateRecord(couponUsedRecord);
            }
        }

        private bool RemoveCouponInternal(CouponApplicabilityContext context) {
            if (context.Coupon != null && context.ShoppingCart != null) {
                if (context.ShoppingCart.PriceAlterations
                    .Any(cap =>
                        CouponingUtilities.CouponAlterationType.Equals(cap.AlterationType, StringComparison.InvariantCultureIgnoreCase)
                        && context.Coupon.Code.Equals(cap.Key, StringComparison.InvariantCultureIgnoreCase))) {
                    // we do that if before actually attempting to remove just so we can give a notification
                    // otherwise we may end up giving it even when we are not removing anything
                    context.ShoppingCart.PriceAlterations = context.ShoppingCart.PriceAlterations
                        .Where(cap =>
                            // Do not remove alterations that are not coupons
                            !CouponingUtilities.CouponAlterationType.Equals(cap.AlterationType, StringComparison.InvariantCultureIgnoreCase)
                            // Use the give key to remove a coupon (if it exists)
                            || !context.Coupon.Code.Equals(cap.Key, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                    return true;
                }
            }
            return false;
        }

        private string GetRemoveActionUrl() {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper.Action("Remove", "Coupon", new { area = "Nwazet.Commerce" });
        }

        private string GetRemoveActionUrl(string code) {
            if (string.IsNullOrWhiteSpace(code)) {
                return GetRemoveActionUrl();
            }
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return urlHelper
                .Action("Remove", "Coupon", new { area = "Nwazet.Commerce" })
                + "?coupon.Code=" + code;
        }

        private void Apply(CouponApplicabilityContext context) {

            // based on the coupon, we add a CartPriceAlteration to the shoppingCart
            // this object will be used in computing the total cart price by the 
            // implementation of ICartPriceAlterationProcessor for coupons.
            // It will also be used by CouponCartExtensionProvider
            // to write to the user that the coupon is "active".
            var allAlterations = new List<CartPriceAlteration> {
                new CartPriceAlteration {
                    AlterationType = CouponingUtilities.CouponAlterationType,
                    Key = context.Coupon.Code,
                    // Higher priority coupons go first. Coupons with the same priority go based
                    // on their value type: CartAmount > Amount > Percent
                    Weight = CouponingUtilities.CouponAlterationWeight(context.Coupon),
                    RemovalAction = GetRemoveActionUrl(context.Coupon.Code)
                }
            };
            if (context.ShoppingCart.PriceAlterations != null) {
                allAlterations.AddRange(context.ShoppingCart.PriceAlterations);
            }
            context.ShoppingCart.PriceAlterations = allAlterations.OrderByDescending(cpa => cpa.Weight).ToList();
        }

        #endregion


        #region Convenience methods
        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;

        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }

        // prevent repeating notifications if more processes test the same stuff
        private HashSet<string> _notificationsSent;

        private void Warning(LocalizedString text) {
            if (!_notificationsSent.Contains(text.Text)) {
                _notifier.Warning(text);
                _notificationsSent.Add(text.Text);
            }
        }
        #endregion
    }
}
