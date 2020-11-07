﻿using Nwazet.Commerce.Models.Couponing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Couponing {
    public class Coupon {
        public Coupon() {
            Id = 0;
            Published = false;
            Value = 0;
            CouponType = CouponType.Percent;
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Public Name of the coupon: e.g. Merry Christmas

        [StringLength(255),Required]
        public string Code { get; set; } // Actual code for the coupon: e.g. XMAS2020

        public bool Published { get; set; }

        public decimal Value { get; set; }

        public CouponType CouponType { get; set; }
    }
}
