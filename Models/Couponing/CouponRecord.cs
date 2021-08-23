using Nwazet.Commerce.Models.Couponing;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponRecord {

        // IMPORTANT
        // Objects of this class are serialized to XML in the 
        // ToXMLElement extension method of CouponingUtilities.
        // When properties are added, changed, removed, make sure to
        // also update that method to avoid unexpected behavior.

        public CouponRecord () {
            ApplicabilityCriteria = new List<CouponApplicabilityCriterionRecord>();
            LineCriteria = new List<CouponLineCriterionRecord>();
        }

        public virtual int Id { get; set; } //Primary Key

        #region Coupon definition
        [StringLengthMax]
        public virtual string Name { get; set; } // Public Name of the coupon: e.g. Merry Christmas
        // TODO: The validation error message for this should be localized
        [StringLength(255)] // 255 is the length for "default" nvarchar on sql server
        public virtual string Code { get; set; } // Actual code for the coupon: e.g. XMAS2020
        // Priority is used to know which coupon should be processed first, since
        // their processing order, when they are of different type (CouponType),
        // affects the final value/price.
        [Range(0, 16383)]
        public virtual int Priority { get; set; }
        #endregion

        #region Conditions: should the coupon apply? Is it "valid"?
        public virtual bool Published { get; set; }

        [CascadeAllDeleteOrphan, Aggregate]
        [XmlArray("ApplicabilityCriteria")]
        public virtual IList<CouponApplicabilityCriterionRecord> ApplicabilityCriteria { get; set; }
        #endregion

        #region Actions: what does the coupon do?
        public virtual decimal Value { get; set; }
        public virtual CouponType CouponType { get; set; }

        [CascadeAllDeleteOrphan, Aggregate]
        [XmlArray("LineCriteria")]
        public virtual IList<CouponLineCriterionRecord> LineCriteria { get; set; }
        #endregion

        public override string ToString() {
            return $"(Coupon {Code}) {Name}: {Value.ToString("#.##")} {CouponType.ToString()}";
        }
    }
    
}
