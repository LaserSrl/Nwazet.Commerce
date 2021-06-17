using Nwazet.Commerce.Descriptors;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public class CouponCriteriaManagementService : ICouponCriteriaManagementService {

        private readonly IEnumerable<ICouponApplicabilityCriterionProvider> _applicabilityCriteriaProviders;
        private readonly IRepository<CouponApplicabilityCriterionRecord> _criteriaRepository;
        private readonly IEnumerable<ICouponLineApplicabilityCriterionProvider> _applicabilityLineCriteriaProviders;
        private readonly IRepository<CouponLineCriterionRecord> _lineCriteriaRepository;

        public CouponCriteriaManagementService(
            IEnumerable<ICouponApplicabilityCriterionProvider> applicabilityCriteriaProviders,
            IRepository<CouponApplicabilityCriterionRecord> criteriaRepository,
            IEnumerable<ICouponLineApplicabilityCriterionProvider> applicabilityLineCriteriaProviders,
            IRepository<CouponLineCriterionRecord> lineCriteriaRepository) {

            _applicabilityCriteriaProviders = applicabilityCriteriaProviders;
            _criteriaRepository = criteriaRepository;
            _applicabilityLineCriteriaProviders = applicabilityLineCriteriaProviders;
            _lineCriteriaRepository = lineCriteriaRepository;
        }

        #region Manage Applicability Criteria
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
    }
}
