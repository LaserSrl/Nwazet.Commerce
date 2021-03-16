using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryPartRecord : ContentPartRecord {
        [Aggregate]
        public virtual TerritoryInternalRecord TerritoryInternalRecord { get; set; }


        [Aggregate]
        public virtual TerritoryPartRecord ParentTerritory { get; set; }
        
        [Aggregate]
        public virtual TerritoryHierarchyPartRecord Hierarchy { get; set; }

        /// <summary>
        /// Compares the Id of internal records
        /// </summary>
        /// <param name="other">The TerritoryPartRecord that will be compared with the current one.</param>
        /// <returns>True if the records have the same Id.</returns>
        public bool IsSameAs(TerritoryPartRecord other) {
            if (this != null && this.TerritoryInternalRecord != null
                && other != null && other.TerritoryInternalRecord != null) {
                return this.TerritoryInternalRecord.Id == other.TerritoryInternalRecord.Id;
            }
            return false;
        }

        public class TerritoryPartRecordComparer : IEqualityComparer<TerritoryPartRecord> {
            public bool Equals(TerritoryPartRecord x, TerritoryPartRecord y) {
                return x.IsSameAs(y);
            }

            public int GetHashCode(TerritoryPartRecord obj) {
                if (obj == null) {
                    return 0;
                }
                return obj.Id;
            }
        }
    }
}
