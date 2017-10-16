using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Territories")]
    public class TerritoryInternalRecord {
        public virtual int Id { get; set; } //Primary Key
        [StringLengthMax]
        public virtual string Name { get; set; } //Name given to the territory

        /// <summary>
        /// Returns a deep copy of the TerritoryInternalRecord passed as parameter.
        /// </summary>
        /// <param name="tir">The object to duplicate</param>
        /// <returns>A deep copy of the TerritoryInternalRecord passed as parameter.</returns>
        public static TerritoryInternalRecord Copy(TerritoryInternalRecord tir) {
            return new TerritoryInternalRecord {
                Id = tir.Id,
                Name = tir.Name
            };
            //this allows us to safely pass stuff alog without affecting the data in the db
        }

        /// <summary>
        /// Returns a deep copy of the TerritoryInternalRecords passed as parameter.
        /// </summary>
        /// <param name="tir">The object to duplicate</param>
        /// <returns>A deep copy of the TerritoryInternalRecords passed as parameter.</returns>
        public static IEnumerable<TerritoryInternalRecord> Copy(IEnumerable<TerritoryInternalRecord> records) {
            var copy = new List<TerritoryInternalRecord>(records.Count());
            copy.AddRange(records.Select(tir => Copy(tir)));
            return copy;
        }
    }
}
