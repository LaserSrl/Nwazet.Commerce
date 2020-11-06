using Nwazet.Commerce.Models.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponRepositoryService : IDependency {

        /// <summary>
        /// Get the total number of CouponRecord objects in the storage.
        /// </summary>
        /// <returns>The total number of objects.</returns>
        int GetCouponsCount();
        /// <summary>
        /// Gets the CouponRecord objects based on the pagination
        /// </summary>
        /// <param name="startIndex">Start index for pagination</param>
        /// <param name="pageSize">Page size (maximum number of objects)</param>
        /// <returns>An IEnumerable of TerritoryInternalRecord objects, that are deep copies 
        /// of the objects in the storage.</returns>
        IEnumerable<CouponRecord> GetCoupons(int startIndex = 0, int pageSize = 0);
    }
}
