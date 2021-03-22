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
        private readonly IEnumerable<ICouponApplicabilityCriterion> _applicabilityCriteria;
        private readonly IEnumerable<ICouponApplicabilityCriterionProvider> _applicabilityCriteriaProviders;
        private readonly IUsedCouponsRepositoryService _usedCouponsRepositoryService;
        private readonly IRepository<CouponApplicabilityCriterionRecord> _criteriaRepository;
        private readonly ITokenizer _tokenizer;

        public CouponApplicationService(
            ICouponRepositoryService couponRepositoryService,
            IWorkContextAccessor workContextAccessor,
            INotifier notifier,
            IEnumerable<ICouponApplicabilityCriterion> applicabilityCriteria,
            IEnumerable<ICouponApplicabilityCriterionProvider> applicabilityCriteriaProviders,
            IUsedCouponsRepositoryService usedCouponsRepositoryService,
            IRepository<CouponApplicabilityCriterionRecord> criteriaRepository,
            ITokenizer tokenizer) {

            _couponRepositoryService = couponRepositoryService;
            _workContextAccessor = workContextAccessor;
            _notifier = notifier;
            _applicabilityCriteria = applicabilityCriteria;
            _applicabilityCriteriaProviders = applicabilityCriteriaProviders;
            _usedCouponsRepositoryService = usedCouponsRepositoryService;
            _criteriaRepository = criteriaRepository;
            _tokenizer = tokenizer;

            _loadedCoupons = new Dictionary<string, CouponRecord>();
            _notificationsSent = new HashSet<string>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        // prevent loading the same coupon several times per request
        private Dictionary<string, CouponRecord> _loadedCoupons;
        private HashSet<string> _notificationsSent;

        private void Warning(LocalizedString text) {
            if (!_notificationsSent.Contains(text.Text)) {
                _notifier.Warning(text);
                _notificationsSent.Add(text.Text);
            }
        }
        
        public void ApplyCoupon(CouponApplicabilityContext context) {
            // given the code, find the coupon
            var coupon = GetCouponFromCode(context.CouponCode);
            if (coupon != null) {
                // given the coupon, check whether it's usable
                // then check whether it applies to the current "transaction"
                context.Coupon = coupon;
                context.IsApplicable = coupon.Published;

                if (CanApply(context)) {

                    Apply(context);
                    _notifier.Information(T("Coupon {0} was successfully applied", context.Coupon.Code));
                }
            } else {
                Warning(T("Coupon code {0} is not valid", context.CouponCode));
            }
        }
        
        private void Apply(CouponApplicabilityContext context) {
            //TODO
            // based on the coupon, we add a CartPriceAlteration to the shoppingCart
            // this object will be used in computing the total cart price by the 
            // implementation of ICartPriceAlterationProcessor for coupons.
            // It will also be used by CouponCartExtensionProvider
            // to write to the user that the coupon is "active".
            var allAlterations = new List<CartPriceAlteration> {
                new CartPriceAlteration {
                    AlterationType = CouponingUtilities.CouponAlterationType,
                    Key = context.Coupon.Code,
                    Weight = 1,
                    RemovalAction = GetRemoveActionUrl(context.Coupon.Code)
                }
            };
            if (context.ShoppingCart.PriceAlterations != null) {
                allAlterations.AddRange(context.ShoppingCart.PriceAlterations);
            }
            context.ShoppingCart.PriceAlterations = allAlterations.OrderByDescending(cpa => cpa.Weight).ToList();
        }

        private CouponRecord GetCouponFromCode(string code) {
            if (!_loadedCoupons.ContainsKey(code)) {
                _loadedCoupons.Add(code,
                    _couponRepositoryService.Query().GetByCode(code));
            }
            return _loadedCoupons[code];
        }
        
        private void InnerTestCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponCriterionContext> descriptorsTest,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest) {

            // TODO: prepare tokens
            Dictionary<string, object> tokens = new Dictionary<string, object>();

            // Some ICouponApplicabilityCriterion will not have a description because
            // they are there by default for all coupons.
            foreach (var criterion in _applicabilityCriteria) {
                defaultTest(criterion, context);
            }
            // After those, we check for the criteria that are configured explicitly
            // for the coupon.
            if (context.IsApplicable) {
                foreach (var criterion in context.Coupon.ApplicabilityCriteria) {
                    var tokenizedState = _tokenizer.Replace(criterion.State, tokens);
                    var criterionContext = new CouponCriterionContext {
                        IsApplicable = context.IsApplicable,
                        ApplicabilityContext = context,
                        State = FormParametersHelper.ToDynamic(tokenizedState),
                        CouponRecord = context.Coupon
                    };
                    var descriptor = GetCriterion(criterion.Category, criterion.Type);
                    // descriptor should exist
                    if (descriptor == null) {
                        continue;
                    }
                    descriptorsTest(descriptor, criterionContext);
                }
            }
        }

        private bool TestCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponCriterionContext> descriptorsTest,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest) {

            if (context.IsApplicable) {
                InnerTestCriteria(context, descriptorsTest, defaultTest);
            }
            if (!context.IsApplicable) {
                if (context.Message != null && !string.IsNullOrWhiteSpace(context.Message.Text)) {
                    Warning(context.Message);
                } else {
                    Warning(T("Coupon code {0} is not valid", context.Coupon.Code));
                }
            }
            return context.IsApplicable;
        }

        private bool CanApply(CouponApplicabilityContext context) {
            return TestCriteria(context,
                (cacd, ccc) => cacd.AdditionCriterion(ccc),
                (cac, ctx) => cac.CanBeAdded(ctx));
        }
        
        public bool CanProcess(CouponApplicabilityContext context) {
            return TestCriteria(context,
                (cacd, ccc) => cacd.ProcessingCriterion(ccc),
                (cac, ctx) => cac.CanBeProcessed(ctx));
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

        public void RemoveCoupon(CouponApplicabilityContext context) {
            if (context.Coupon == null) {
                context.Coupon = GetCouponFromCode(context.CouponCode);
            }
            if (RemoveCouponInternal(context)) {
                _notifier.Information(T("Coupon {0} was removed", context.CouponCode));
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

        public void ReevaluateValidity(CouponLifeUpdateContext context) {
            // TODO add to context a flag telling us whether we should remove
            // coupons that can't be processed anymore

            var applicabilityContext = new CouponApplicabilityContext {
                Coupon = context.Coupon,
                ShoppingCart = context.ShoppingCart,
                WorkContext = context.WorkContext,
                IsApplicable = context.Coupon.Published
            };
            if (!CanProcess(applicabilityContext)) {
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
                couponUsedRecord.WasInvalid = !CanProcess(applicabilityContext);

                // TODO: providers to set the values of
                // couponUsedRecord.AdditionalUserIdentifier
                // and
                // couponUsedRecord.IdentifierType
                // The providers should be put in a property of context so they can be passed
                // around to whatever code needs them.

                // that CouponUsedRecord should actually be saved in the db
                _usedCouponsRepositoryService.CreateRecord(couponUsedRecord);
            }
        }

        public IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>>
            DescribeApplicabilityCriteria() {

            var context = new DescribeCouponApplicabilityContext();

            foreach (var provider in _applicabilityCriteriaProviders) {
                provider.Describe(context);
            }

            return context.Describe();
        }

        public CouponApplicabilityCriterionDescriptor
            GetCriterion(string category, string type) {

            return DescribeApplicabilityCriteria()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(c =>
                    c.Category == category
                    && c.Type == type
                );
        }

        public void DeleteCriterion(int criterionId) {
            var record = _criteriaRepository.Get(criterionId);
            if (record != null) {
                DeleteCriterion(record);
            }
        }

        private void DeleteCriterion(CouponApplicabilityCriterionRecord record) {
            record.CouponRecord.ApplicabilityCriteria.Remove(record);
            _criteriaRepository.Delete(record);
        }
    }
}
