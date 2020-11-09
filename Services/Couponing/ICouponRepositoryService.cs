using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Couponing {
    public interface ICouponRepositoryService : IDependency {

        IQueryable<CouponRecord> Query();
        /// <summary>
        /// Retrieve from storage the record with the id, and returns a coupon entity
        /// </summary>
        /// <param name="id">The id to retrieve</param>
        /// <returns>Coupon</returns>
        Coupon Get(int id);

        /// <summary>
        /// Create a persistent record starting from a coupon
        /// </summary>
        /// <param name="coupon">The coupon to create</param>
        /// <returns></returns>

        int CreateRecord(Coupon coupon);
        /// <summary>
        /// Get the record having the same id of the coupon and update it with the coupon data
        /// </summary>
        /// <param name="coupon">The coupon to be updated</param>
        void UpdateRecord(Coupon coupon);
        /// <summary>
        /// Delete the record 
        /// </summary>
        /// <param name="id">the record id to delete</param>
        void DeleteRecord(int id);
    }
}
