using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Recipes.Providers.Builders {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponStep : RecipeBuilderStep {
        private readonly ICouponRepositoryService _couponRepositoryService;
        private readonly ICouponCriteriaManagementService _couponCriteriaManagementService;

        public CouponStep(
            ICouponRepositoryService couponRepositoryService,
            ICouponCriteriaManagementService couponCriteriaManagementService) {

            _couponRepositoryService = couponRepositoryService;
            _couponCriteriaManagementService = couponCriteriaManagementService;
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
            // TODO: create an XML element for the coupons and store each information there
        }

        private CouponApplicabilityCriterionEntry CriterionToEntry(
            CouponCriterionBaseRecord criterion, CouponCriterionDescriptor descriptor) {
            return new CouponApplicabilityCriterionEntry {
                Category = descriptor.Category,
                Type = descriptor.Type,
                CriterionRecordId = criterion.Id,
                DisplayText = string.IsNullOrWhiteSpace(criterion.Description)
                    ? descriptor.Display(new CouponContext {
                        State = FormParametersHelper.ToDynamic(criterion.State)
                    }).Text
                    : criterion.Description,
                IsAvailableForConfiguration = descriptor.IsAvailableForConfiguration,
                IsAvailableForProcessing = descriptor.IsAvailableForProcessing
            };
        }
    }
}
