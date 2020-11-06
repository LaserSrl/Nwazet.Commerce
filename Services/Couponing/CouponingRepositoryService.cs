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

namespace Nwazet.Commerce.Services.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingRepositoryService : ICouponRepositoryService {

        private readonly IRepository<CouponRecord> _couponsRepository;

        public CouponingRepositoryService(
            IRepository<CouponRecord> couponsRepository) {

            _couponsRepository = couponsRepository;
        }

        public IEnumerable<CouponRecord> GetCoupons(int startIndex = 0, int pageSize = 0) {
            var result = _couponsRepository.Table
                .Skip(startIndex >= 0 ? startIndex : 0);

            if (pageSize > 0) {
                return result.Take(pageSize).CreateSafeDuplicate();
            }
            return result.ToList().CreateSafeDuplicate();
        }

        public int GetCouponsCount() {
            return _couponsRepository.Table.Count();
        }
    }
}
