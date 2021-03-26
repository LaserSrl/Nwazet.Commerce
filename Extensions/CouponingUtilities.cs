using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Nwazet.Commerce.ViewModels.Couponing;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Extensions {
    [OrchardFeature("Nwazet.Couponing")]
    public static class CouponingUtilities {

        public const string CouponAlterationType = "Coupon";
        /// <summary>
        /// Use this as attribute name when we need to get the
        /// name of the coupon from the XML.
        /// </summary>
        public const string CouponAttributeName = "Code";

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
            if (record == null) {
                return null;
            }
            return new Coupon {
                Id = record.Id,
                Name = record.Name,
                Code = record.Code,
                Value = record.Value,
                CouponType = record.CouponType,
                Published = record.Published,
                Record = record
            };

        }

        public static void FromCoupon(this CouponRecord record, Coupon coupon) {
            record.Name = coupon.Name;
            record.Code = coupon.Code;
            record.Value = coupon.Value;
            record.CouponType = coupon.CouponType;
            record.Published = coupon.Published;
        }
        /// <summary>
        /// Create an XElement for the CouponRecord. The optional parameter
        /// tells whether the coupon was processable and thus used in the
        /// context where this metohd is called.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="wasInvalid"></param>
        /// <returns></returns>
        /// <remarks>When serializing coupons for an order, set wasInvalid = true
        /// to indicate those coupons that were in the cart/order, but had no
        /// effect.</remarks>
        public static XElement ToXMLElement(this CouponRecord record, bool wasInvalid = false) {
            // The record's properties are serialized as attributes of the
            // resulting XElement. This will result in an XML element that looks
            // like this:
            // <Coupon Name="name" Code="code" {...more attributes} />
            var couponEl = new XElement(CouponAlterationType)
                .With(record)
                // definition
                .ToAttr(c => c.Name)
                .ToAttr(c => c.Code)
                // conditions
                .ToAttr(c => c.Published)
                // should also serialize
                // actions
                .ToAttr(c => c.Value)
                .ToAttr(c => c.CouponType)
                ;
            if (wasInvalid) {
                couponEl.Element.SetAttributeValue("WasInvalid", wasInvalid);
            }
            couponEl.Element
                .AddEl(record.ApplicabilityCriteria
                    .Select(ac => {
                        var criterionEl = new XElement("CouponApplicabilityCriterion")
                            .Attr("Id", ac.Id)
                            .Attr("Category", ac.Category)
                            .Attr("Description", ac.Description)
                            .Attr("Type", ac.Type);
                        // State is already serialized as XML
                        criterionEl.SetElementValue("State", ac.State);
                        return criterionEl;
                    })
                    .ToArray());
            return couponEl;
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

        public static CouponRecord GetByCode(this IQueryable<CouponRecord> table, string code, bool published = true) {
            if (published) {
                return table.IsPublished().SingleOrDefault(x => x.Code.ToLowerInvariant().Equals(code.ToLowerInvariant()));
            }
            else {
                return table.SingleOrDefault(x => x.Code.ToLowerInvariant().Equals(code.ToLowerInvariant()));
            }
        }

        #endregion

    }
}
