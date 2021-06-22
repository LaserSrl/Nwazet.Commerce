using Nwazet.Commerce.ApplicabilityCriteria.Couponing;
using Nwazet.Commerce.Descriptors.CouponApplicability;
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

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponEvaluationService : ICouponEvaluationService {

        private readonly INotifier _notifier;
        private readonly IEnumerable<ICouponApplicabilityCriterion> _applicabilityCriteria;
        private readonly ITokenizer _tokenizer;
        private readonly ICouponCriteriaManagementService _couponCriteriaManagementService;

        public CouponEvaluationService(
            INotifier notifier,
            IEnumerable<ICouponApplicabilityCriterion> applicabilityCriteria,
            ITokenizer tokenizer,
            ICouponCriteriaManagementService couponCriteriaManagementService) {

            _notifier = notifier;
            _applicabilityCriteria = applicabilityCriteria;
            _tokenizer = tokenizer;
            _couponCriteriaManagementService = couponCriteriaManagementService;

            _notificationsSent = new HashSet<string>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanProcess(CouponApplicabilityContext context) {
            return TestCriteria(context,
                (cacd, ccc) => cacd.ProcessingCriterion(ccc),
                (cac, ctx) => cac.CanBeProcessed(ctx));
        }

        public bool CanApply(CouponApplicabilityContext context) {
            return TestCriteria(context,
                (cacd, ccc) => cacd.AdditionCriterion(ccc),
                (cac, ctx) => cac.CanBeAdded(ctx));
        }

        #region Methods to test validity/applicability of coupon
        private bool TestCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponApplicabilityCriterionContext> descriptorsTest,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest) {

            // Some ICouponApplicabilityCriterion will not have a description because
            // they are there by default for all coupons.
            if (context.IsApplicable) {
                BaseTestCriteria(context, defaultTest);
            }
            // After those, we check for the criteria that are configured explicitly
            // for the coupon.
            if (context.IsApplicable) {
                TestApplicabilityCriteria(context, descriptorsTest);
            }
            // Then we need to evaluate all LineCriteria that are configured for the 
            // coupon. Each criterion has to succeed for at least 1 line of the cart
            if (context.IsApplicable) {
                TestLineCriteria(context);
            }
            if (!context.IsApplicable && context.ShouldNotify) {
                if (context.Message == null || string.IsNullOrWhiteSpace(context.Message.Text)) {
                    context.Message = T("Coupon code {0} is not valid", context.Coupon.Code);
                }

                Warning(context.Message);
            }
            return context.IsApplicable;
        }

        private void BaseTestCriteria(
            CouponApplicabilityContext context,
            Action<ICouponApplicabilityCriterion, CouponApplicabilityContext> defaultTest) {

            // Some ICouponApplicabilityCriterion will not have a description because
            // they are there by default for all coupons.
            foreach (var criterion in _applicabilityCriteria) {
                defaultTest(criterion, context);
            }
        }

        private void TestApplicabilityCriteria(
            CouponApplicabilityContext context,
            Action<CouponApplicabilityCriterionDescriptor, CouponApplicabilityCriterionContext> descriptorsTest) {

            // TODO: prepare tokens
            Dictionary<string, object> tokens = new Dictionary<string, object>();
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
                    var descriptor = _couponCriteriaManagementService
                        .GetCriterion(criterion.Category, criterion.Type);
                    // descriptor should exist and be enabled
                    if (descriptor == null || !descriptor.IsAvailableForProcessing) {
                        continue;
                    }
                    descriptorsTest(descriptor, criterionContext);
                }
            }
        }

        private void TestLineCriteria(
            CouponApplicabilityContext context) {
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
                    var descriptor = _couponCriteriaManagementService
                        .GetLineCriterion(criterion.Category, criterion.Type);
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

        #endregion

        #region Helpers

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
