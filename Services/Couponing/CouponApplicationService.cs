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
        private readonly IEnumerable<ICouponLineApplicabilityCriterionProvider> _applicabilityLineCriteriaProviders;
        private readonly IUsedCouponsRepositoryService _usedCouponsRepositoryService;
        private readonly IRepository<CouponApplicabilityCriterionRecord> _criteriaRepository;
        private readonly IRepository<CouponLineCriterionRecord> _lineCriteriaRepository;
        private readonly ITokenizer _tokenizer;
        private readonly IEnumerable<ICouponUserIdentifierProvider> _couponUserIdentifierProviders;

        public CouponApplicationService(
            ICouponRepositoryService couponRepositoryService,
            IWorkContextAccessor workContextAccessor,
            INotifier notifier,
            IEnumerable<ICouponApplicabilityCriterion> applicabilityCriteria,
            IEnumerable<ICouponApplicabilityCriterionProvider> applicabilityCriteriaProviders,
            IEnumerable<ICouponLineApplicabilityCriterionProvider> applicabilityLineCriteriaProviders,
            IUsedCouponsRepositoryService usedCouponsRepositoryService,
            IRepository<CouponApplicabilityCriterionRecord> criteriaRepository,
            IRepository<CouponLineCriterionRecord> lineCriteriaRepository,
            ITokenizer tokenizer,
            IEnumerable<ICouponUserIdentifierProvider> couponUserIdentifierProviders) {

            _couponRepositoryService = couponRepositoryService;
            _workContextAccessor = workContextAccessor;
            _notifier = notifier;
            _applicabilityCriteria = applicabilityCriteria;
            _applicabilityCriteriaProviders = applicabilityCriteriaProviders;
            _applicabilityLineCriteriaProviders = applicabilityLineCriteriaProviders;
            _usedCouponsRepositoryService = usedCouponsRepositoryService;
            _criteriaRepository = criteriaRepository;
            _lineCriteriaRepository = lineCriteriaRepository;
            _tokenizer = tokenizer;
            _couponUserIdentifierProviders = couponUserIdentifierProviders
                .OrderByDescending(cuip => cuip.Priority);

            _loadedCoupons = new Dictionary<string, CouponRecord>();
            _notificationsSent = new HashSet<string>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void InnerTestCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponApplicabilityCriterionContext> descriptorsTest,
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
                    var criterionContext = new CouponApplicabilityCriterionContext {
                        IsApplicable = context.IsApplicable,
                        ApplicabilityContext = context,
                        State = FormParametersHelper.ToDynamic(tokenizedState),
                        CouponRecord = context.Coupon
                    };
                    var descriptor = GetCriterion(criterion.Category, criterion.Type);
                    // descriptor should exist and be enabled
                    if (descriptor == null || !descriptor.IsAvailableForProcessing) {
                        continue;
                    }
                    descriptorsTest(descriptor, criterionContext);
                }
            }
            
        }

        private bool TestCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponApplicabilityCriterionContext> descriptorsTest,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest) {

            if (context.IsApplicable) {
                InnerTestCriteria(context, descriptorsTest, defaultTest);
            }
            // Then we need to evaluate all LineCriteria that are configured for the 
            // coupon. Each criterion has to succeed for at least 1 line of the cart
            if (context.IsApplicable) {
                // TODO: prepare tokens
                Dictionary<string, object> tokens = new Dictionary<string, object>();
                // We need to test for each line. Note that this method, if the context
                // is defined for a specific line already, returns itself rather than 
                // a list of contexts for every cart line. We force a ToList() there to
                // force enumerating, so we have the actual objects rather than a reference
                // to how to get them, because otherwise the wrong references may be passed
                // around at later steps (basically, the providers would change the 
                // IsApplicable for an object, then a fresh one would be checked of the
                // flag's value).
                var lineContexts = context.ContextsForLines().ToList();
                foreach (var lineApplicabilityContext in lineContexts) {
                    foreach (var criterion in context.Coupon.LineCriteria) {
                        var descriptor = GetLineCriterion(criterion.Category, criterion.Type);
                        // descriptor should exist and be enabled
                        if (descriptor == null || !descriptor.IsAvailableForProcessing) {
                            continue;
                        }

                        var tokenizedState = _tokenizer.Replace(criterion.State, tokens);
                        var lineCriterionContext = new CouponLineCriterionContext {
                            IsApplicable = lineApplicabilityContext.IsApplicable,
                            ApplicabilityContext = lineApplicabilityContext,
                            State = FormParametersHelper.ToDynamic(tokenizedState),
                            CouponRecord = lineApplicabilityContext.Coupon
                        };
                        descriptor.Criterion(lineCriterionContext);
                        // break as soon as we know this line is not ok for the coupon
                        if (!lineCriterionContext.IsApplicable) {
                            context.Message = descriptor.FailureMessage(context);
                            // go to test next line
                            break;
                        }
                    }
                }
                // If the criterion fails for all lines, break out
                if (!lineContexts.Any(lctx => lctx.IsApplicable)) {
                    context.IsApplicable = false;
                }
            }
            if (!context.IsApplicable && context.ShouldNotify) {
                if (context.Message == null || string.IsNullOrWhiteSpace(context.Message.Text)) {
                    context.Message = T("Coupon code {0} is not valid", context.Coupon.Code);
                }

                Warning(context.Message);
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

        #region Coupon lifecycle

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

        #endregion

        #region Manage Applicability
        private IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>>
            InnerDescribeApplicabilityCriteria() {

            var context = new DescribeCouponApplicabilityContext();

            foreach (var provider in _applicabilityCriteriaProviders) {
                provider.Describe(context);
            }

            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<CouponApplicabilityCriterionDescriptor>>
            DescribeApplicabilityCriteria() {

            var fullSet = InnerDescribeApplicabilityCriteria();
            var filteredSet = fullSet
                .Select(td => new TypeDescriptor<CouponApplicabilityCriterionDescriptor>() {
                    Category = td.Category,
                    Name = td.Name,
                    Description = td.Description,
                    Descriptors = td.Descriptors.Where(cacd => cacd.IsAvailableForConfiguration)
                })
                .Where(td => td.Descriptors.Any());
            return filteredSet;
        }

        public CouponApplicabilityCriterionDescriptor
            GetCriterion(string category, string type) {

            return InnerDescribeApplicabilityCriteria()
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
        #endregion

        #region Manage Line Conditions

        private IEnumerable<TypeDescriptor<CouponLineApplicabilityCriterionDescriptor>>
            InnerDescribeLineCriteria() {

            var context = new DescribeCouponLineApplicabilityContext();

            foreach (var provider in _applicabilityLineCriteriaProviders) {
                provider.Describe(context);
            }

            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<CouponLineApplicabilityCriterionDescriptor>>
            DescribeLineCriteria() {

            var fullSet = InnerDescribeLineCriteria();
            var filteredSet = fullSet
                .Select(td => new TypeDescriptor<CouponLineApplicabilityCriterionDescriptor>() {
                    Category = td.Category,
                    Name = td.Name,
                    Description = td.Description,
                    Descriptors = td.Descriptors.Where(cacd => cacd.IsAvailableForConfiguration)
                })
                .Where(td => td.Descriptors.Any());
            return filteredSet;
        }

        public CouponLineApplicabilityCriterionDescriptor
            GetLineCriterion(string category, string type) {

            return InnerDescribeLineCriteria()
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(c =>
                    c.Category == category
                    && c.Type == type
                );
        }

        public void DeleteLineCriterion(int criterionId) {
            var record = _lineCriteriaRepository.Get(criterionId);
            if (record != null) {
                DeleteLineCriterion(record);
            }
        }

        private void DeleteLineCriterion(CouponLineCriterionRecord record) {
            record.CouponRecord.LineCriteria.Remove(record);
            _lineCriteriaRepository.Delete(record);
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
