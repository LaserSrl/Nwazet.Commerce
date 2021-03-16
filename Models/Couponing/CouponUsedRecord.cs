using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponUsedRecord {
        public virtual int Id { get; set; } //Primary Key
        
        /// <summary>
        /// Id of the CouponRecord
        /// </summary>
        public virtual int CouponRecord_Id { get; set; }
        /// <summary>
        /// Id of the user. May be 0 for anonymous users.
        /// </summary>
        public virtual int UserPartRecord_Id { get; set; }
        /// <summary>
        /// Id of the Record created for the order.
        /// </summary>
        public virtual int OrderPartRecord_Id { get; set; }

        /// <summary>
        /// When was the couopn used?
        /// </summary>
        public virtual DateTime DateTimeUTC { get; set; }

        /// <summary>
        /// Additional information used to identify the user of the context.
        /// Value for this comes from an ordered list of providers, each able to propose
        /// a value for this. For example this may contain a device's UUID
        /// </summary>
        [StringLength(255)] // 255 is the length for "default" nvarchar on sql server
        public virtual string AdditionalUserIdentifier { get; set; }
        /// <summary>
        /// String used to identify the meaning of AdditionalUserIdentifier. Each provider for the
        /// value of AdditionalUserIdentifier should have its own unique value of ContextType that
        /// will allow to find the same provider in the future. For example "CallerUUID"
        /// </summary>
        public virtual string IdentifierType { get; set; }
    }
}
