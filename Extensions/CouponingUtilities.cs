using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Extensions {
    [OrchardFeature("Nwazet.Couponing")]
    public static class CouponingUtilities {

        /// <summary>
        /// Returns a copy of the CouponRecords
        /// </summary>
        /// <param name="records"></param>
        /// <returns>A copy of the IEnumerable whose elements can be safely manipulated without affecting 
        /// records in the database.</returns>
        public static IEnumerable<Coupon> ToCoupon(
            this IEnumerable<CouponRecord> records) {

            var copy = new List<Coupon>(records.Count());
            copy.AddRange(records.Select(tir => tir.ToCoupon()));
            return copy;
        }

        public static Coupon ToCoupon(this CouponRecord record) {
            return new Coupon {
                Id = record.Id,
                Name = record.Name,
                Code = record.Code,
                Value = record.Value,
                CouponType = record.CouponType,
                Published = record.Published
            };

        }

        public static void FromCoupon(this CouponRecord record, Coupon coupon) {
            record.Name = coupon.Name;
            record.Code = coupon.Code;
            record.Value = coupon.Value;
            record.CouponType = coupon.CouponType;
            record.Published = coupon.Published;
        }

        #region [IQueryable]
        public static IQueryable<CouponRecord> Paginate(this IQueryable<CouponRecord> table, int startIndex = 0, int pageSize = 0) {
            var result = table
                .Skip(startIndex >= 0 ? startIndex : 0);

            if (pageSize > 0) {
                return result.Take(pageSize);
            }
            return result;
        }

        public static IQueryable<CouponRecord> IsPublished(this IQueryable<CouponRecord> table) {
            return table.Where(x => x.Published);
        }
        #endregion

    }
}
