using Nwazet.Commerce.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface IUsedCouponsRepositoryService : IDependency {
        IQueryable<CouponUsedRecord> Query();
        int CreateRecord(CouponUsedRecord couponUsedRecord);
    }
}
