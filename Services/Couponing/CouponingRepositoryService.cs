using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models.Couponing;
using Orchard.Data;
using Nwazet.Commerce.Extensions;
using Orchard.Environment.Extensions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.ContentManagement;
using Orchard;
using System.Globalization;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingRepositoryService : ICouponRepositoryService {

        private readonly IRepository<CouponRecord> _couponsRepository;
        private readonly IWorkContextAccessor _workContextAccessor;

        private readonly Lazy<CultureInfo> _cultureInfo;

        public CouponingRepositoryService(
            IRepository<CouponRecord> couponsRepository,
            IWorkContextAccessor workContextAccessor) {

            _couponsRepository = couponsRepository;
            _workContextAccessor = workContextAccessor;

            _cultureInfo = new Lazy<CultureInfo>(() => 
                CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));
        }

        public IQueryable<CouponRecord> Query() {
            return _couponsRepository.Table;
        }

        public Coupon Get(int id) {
            var coupon = _couponsRepository.Get(id).ToCoupon(_cultureInfo.Value);
            return coupon;
        }

        public int CreateRecord(Coupon coupon) {
            var record = new CouponRecord();
            record.FromCoupon(coupon, _cultureInfo.Value);
            _couponsRepository.Create(record);
            return record.Id;
        }

        public void UpdateRecord(Coupon coupon) {
            var record = _couponsRepository.Get(coupon.Id);
            record.FromCoupon(coupon, _cultureInfo.Value);
        }

        public void DeleteRecord(int id) {
            var record = _couponsRepository.Get(id);
            _couponsRepository.Delete(record);
        }

        public bool Validate(Coupon coupon) {
            var isValid = CheckUnicity(coupon);

            return isValid;
        }
        private bool CheckUnicity(Coupon coupon) {
            return Query()
                .Any(x => 
                    x.Code.ToLowerInvariant().Equals(coupon.Code.ToLowerInvariant()) 
                    && x.Id != coupon.Id) == false;
        }
    }
}
