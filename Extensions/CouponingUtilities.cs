using Nwazet.Commerce.Models.Couponing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Extensions {
    public static class CouponingUtilities {

        /// <summary>
        /// Returns a copy of the CouponRecords
        /// </summary>
        /// <param name="records"></param>
        /// <returns>A copy of the IEnumerable whose elements can be safely manipulated without affecting 
        /// records in the database.</returns>
        public static IEnumerable<CouponRecord> CreateSafeDuplicate(
            this IEnumerable<CouponRecord> records) {

            var copy = new List<CouponRecord>(records.Count());
            copy.AddRange(records.Select(tir => tir.CreateSafeDuplicate()));
            return copy;
        }
    }
}
