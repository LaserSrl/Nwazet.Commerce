using Nwazet.Commerce.Models.Couponing;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponRecord {

        // IMPORTANT
        // Objects of this class are serialized to XML in the 
        // ToXMLElement extension method of CouponingUtilities.
        // When properties are added, changed, removed, make sure to
        // also update that method to avoid unexpected behavior.

        public virtual int Id { get; set; } //Primary Key

        #region Coupon definition
        [StringLengthMax]
        public virtual string Name { get; set; } // Public Name of the coupon: e.g. Merry Christmas
        // TODO: The validation erro message for this should be localized
        [StringLength(255)] // 255 is the length for "default" nvarchar on sql server
        public virtual string Code { get; set; } // Actual code for the coupon: e.g. XMAS2020
        #endregion

        #region Conditions: should the coupon apply? Is it "valid"?
        public virtual bool Published { get; set; }
        #endregion

        #region Actions: what does the coupon do?
        public virtual decimal Value { get; set; }
        public virtual CouponType CouponType { get; set; }
        #endregion


        public override string ToString() {
            return $"(Coupon {Code}) {Name}: {Value.ToString("#.##")} {CouponType.ToString()}";
        }
    }
    
}
