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
        public virtual int Id { get; set; } //Primary Key

        #region Coupon definition
        [StringLengthMax]
        public virtual string Name { get; set; } // Public Name of the coupon: e.g. Merry Christmas
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
        
    }
    
}
