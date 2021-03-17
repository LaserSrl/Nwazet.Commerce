using Nwazet.Commerce.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class UsedCouponsRepositoryService : IUsedCouponsRepositoryService {

        private readonly IRepository<CouponUsedRecord> _couponsUsedRepository;

        public UsedCouponsRepositoryService(
            IRepository<CouponUsedRecord> couponsUsedRepository) {

            _couponsUsedRepository = couponsUsedRepository;
        }

        public IQueryable<CouponUsedRecord> Query() {
            return _couponsUsedRepository.Table;
        }
        public int CreateRecord(CouponUsedRecord couponUsedRecord) {
            _couponsUsedRepository.Create(couponUsedRecord);
            return couponUsedRecord.Id;
        }

    }
}
