using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Aspects {
    public interface ITerritoryAddressAspect : IContent {
        /// <summary>
        /// This is an ordered collection of the Ids of the 
        /// TerritoryInternalRecord objects to consider.
        /// Ordering within this collection should be such that each
        /// element has higher "priority"/specificity than the next.
        /// For example, considering an address, the city is more 
        /// specific than the province, that is more specific than 
        /// the country. Hence the returned colection for such example
        /// would look like:
        /// { cityId, provinceId, countryId }
        /// </summary>
        IEnumerable<int> TerritoriesIds { get; }
        /// <summary>
        /// This is the id of the TerritoryInternalRecord object to 
        /// consider as country for the address.
        /// </summary>
        int CountryId { get; }
        /// <summary>
        /// This is the id of the TerritoryInternalRecord object to 
        /// consider as province/state for the address.
        /// </summary>
        int ProvinceId { get; }
        /// <summary>
        /// This is the id of the TerritoryInternalRecord object to 
        /// consider as city for the address.
        /// </summary>
        int CityId { get; }
    }
}
