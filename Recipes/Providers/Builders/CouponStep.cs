using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Recipes.Providers.Builders {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponStep : RecipeBuilderStep {
        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly ICouponCriteriaManagementService _couponCriteriaManagementService;
        private readonly IWorkContextAccessor _workContextAccessor;

        private readonly Lazy<CultureInfo> _cultureInfo;

        public CouponStep(
            ICouponRepositoryService couponRepositoryService,
            ICouponCriteriaManagementService couponCriteriaManagementService,
            IWorkContextAccessor workContextAccessor) {

            _couponRepositoryService = couponRepositoryService;
            _couponCriteriaManagementService = couponCriteriaManagementService;
            _workContextAccessor = workContextAccessor;

            _cultureInfo = new Lazy<CultureInfo>(() =>
                CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));
        }

        public override string Name => "Coupons";

        public override LocalizedString DisplayName => T("Coupons");

        public override LocalizedString Description => T("Export configured coupons. This does not export corresponding settings.");

        public override void Build(BuildContext context) {
            var records = _couponRepositoryService
                .Query().ToList();
            var coupons = new List<Coupon>();
            foreach (var record in records) {
                var coupon = _couponRepositoryService.Get(record.Id);
                if (coupon != null) {
                    // The following information is in the CouponRecord. Here we are 
                    // "translating" it to the vms.
                    // populate the vm "summaries" for the criteria
                    foreach (var crit in coupon.Record.ApplicabilityCriteria) {
                        var descriptor = _couponCriteriaManagementService
                            .GetCriterion(crit.Category, crit.Type);
                        if (descriptor != null) {
                            coupon.ApplicabilityCriteria.Add(
                                CriterionToEntry(crit, descriptor));
                        }
                    }
                    // populate the vm "summaries" for the line conditions
                    foreach (var crit in coupon.Record.LineCriteria) {
                        var descriptor = _couponCriteriaManagementService
                            .GetLineCriterion(crit.Category, crit.Type);
                        if (descriptor != null) {
                            coupon.LineCriteria.Add(
                                CriterionToEntry(crit, descriptor));
                        }
                    }
                    coupons.Add(coupon);
                }
            }
            // create an XML element for the coupons and store each information there
            var orchardElement = context.RecipeDocument.Element("Orchard");
            var couponsRoot = new XElement("Coupons");
            foreach (var coupon in coupons) {
                // Value is a string, but it represents a number: we export it
                // in the invariant culture
                decimal value = 0.0m;
                decimal.TryParse(coupon.Value, NumberStyles.Any, _cultureInfo.Value, out value);
                var couponElement = new XElement("Coupon",
                    new XAttribute("Name", coupon.Name),
                    new XAttribute("Code", coupon.Code),
                    new XAttribute("Priority", coupon.Priority),
                    new XAttribute("Published", coupon.Published),
                    new XAttribute("CouponType", coupon.CouponType),
                    new XAttribute("Value", value.ToString(CultureInfo.InvariantCulture)));
                // Applicability criteria
                foreach (var criterion in coupon.ApplicabilityCriteria) {
                    var criterionElement = new XElement("ApplicabilityCriterion",
                        new XAttribute("Category", criterion.Category),
                        new XAttribute("Type", criterion.Type),
                        new XAttribute("DisplayText", criterion.DisplayText),
                        new XAttribute("State", criterion.State));
                    couponElement.Add(criterionElement);
                }
                // Line criteria
                foreach (var criterion in coupon.LineCriteria) {
                    var criterionElement = new XElement("LineCriterion",
                        new XAttribute("Category", criterion.Category),
                        new XAttribute("Type", criterion.Type),
                        new XAttribute("DisplayText", criterion.DisplayText),
                        new XAttribute("State", criterion.State));
                    couponElement.Add(criterionElement);
                }
                couponsRoot.Add(couponElement);
            }
            orchardElement.Add(couponsRoot);
        }

        private CouponApplicabilityCriterionEntry CriterionToEntry(
            CouponCriterionBaseRecord criterion, CouponCriterionDescriptor descriptor) {
            return new CouponApplicabilityCriterionEntry {
                Category = descriptor.Category,
                Type = descriptor.Type,
                CriterionRecordId = criterion.Id,
                DisplayText = criterion.Description ?? "",
                State = criterion.State,
                IsAvailableForConfiguration = descriptor.IsAvailableForConfiguration,
                IsAvailableForProcessing = descriptor.IsAvailableForProcessing
            };
        }
    }
}
