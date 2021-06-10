using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Couponing;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    [OrchardFeature("Nwazet.Couponing")]
    public class Coupon {
        public Coupon() {
            Id = 0;
            Published = false;
            Value = "0.0";
            CouponType = CouponType.Percent;
            ApplicabilityCriteria = new List<CouponApplicabilityCriterionEntry>();
            LineCriteria = new List<CouponApplicabilityCriterionEntry>();
        }

        public CouponRecord Record { get; set; }

        #region Definition
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Public Name of the coupon: e.g. Merry Christmas

        [StringLength(255),Required]
        [RegularExpression(@"[a-zA-Z0-9]{1,255}")]
        public string Code { get; set; } // Actual code for the coupon: e.g. XMAS2020

        [Range(0, 16383)]
        public int Priority { get; set; }
        #endregion
        #region Conditions
        public bool Published { get; set; }
        public List<CouponApplicabilityCriterionEntry> ApplicabilityCriteria { get; set; }
        #endregion
        #region Actions
        public CouponType CouponType { get; set; }
        public string Value { get; set; }

        public List<CouponApplicabilityCriterionEntry> LineCriteria { get; set; }
        #endregion
    }
}
