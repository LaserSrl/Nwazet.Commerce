using Nwazet.Commerce.Exceptions;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

//TODO: Create a service for having detached record as a "SafeDuplicate" see CreateSafeDuplicate;
//      RepositoryService should have record attached to the Session
namespace Nwazet.Commerce.Services {
    [OrchardFeature("Territories")]
    public class TerritoryRepositoryService : ITerritoriesRepositoryService {

        private readonly IRepository<TerritoryInternalRecord> _territoryInternalRecord;

        public TerritoryRepositoryService(
            IRepository<TerritoryInternalRecord> territoryInternalRecord) {

            _territoryInternalRecord = territoryInternalRecord;

            T = NullLocalizer.Instance;

            _hashAlgorithm = TerritoriesUtilities.GetHashAlgorithm();
        }

        public Localizer T;
        HashAlgorithm _hashAlgorithm;

        public TerritoryInternalRecord GetTerritoryInternal(int id) {
            var tir = _territoryInternalRecord.Get(id);
            return tir == null ? null :
                tir.CreateSafeDuplicate();
        }

        public TerritoryInternalRecord GetTerritoryInternal(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return null;
            }
            name = name.Trim();
            var tir = ExistingRecord(name);
            return tir == null ? null :
                tir.CreateSafeDuplicate();
        }

        public IEnumerable<TerritoryInternalRecord> GetTerritories(int startIndex = 0, int pageSize = 0) {
            var result = _territoryInternalRecord.Table
                .Skip(startIndex >= 0 ? startIndex : 0);

            if (pageSize > 0) {
                return result.Take(pageSize);
            }
            return result;
        }

        public IEnumerable<TerritoryInternalRecord> GetTerritories(int[] itemIds) {
            if (itemIds.Any()) {
                return _territoryInternalRecord
                    .Fetch(x => itemIds.Contains(x.Id));
            }
            return Enumerable.Empty<TerritoryInternalRecord>();
        }

        public IEnumerable<TerritoryInternalRecord> GetTerritories(string[] itemIds) {
            if (itemIds.Any()) {
                var hashes = itemIds.Select(n => GetHash(n)).ToArray();
                return _territoryInternalRecord
                    .Fetch(x => hashes.Contains(x.NameHash))
                    .Where(x => itemIds.Contains(x.Name));
            }
            return Enumerable.Empty<TerritoryInternalRecord>();
        }

        public int GetTerritoriesCount() {
            return _territoryInternalRecord.Table.Count();
        }

        public TerritoryInternalRecord AddTerritory(TerritoryInternalRecord tir) {
            ValidateTir(tir);
            tir.Name = tir.Name.Trim();
            tir.NameHash = GetHash(tir.Name);
            if (GetSameNameIds(tir).Any()) {
                throw new TerritoryInternalDuplicateException(T("Cannot create duplicate names. A territory with the same name already exists."));
            }
            _territoryInternalRecord.Create(tir);
            return tir.CreateSafeDuplicate();
        }

        public bool TryAddTerritory(string name) {
            // name must be valid
            if (string.IsNullOrWhiteSpace(name)) {
                return false;
            }
            var actualName = name.Trim();
            // duplicates aren't allowed
            var existing = ExistingRecord(actualName);
            if (existing != null) {
                return false;
            }
            //actually create
            _territoryInternalRecord.Create(
                new TerritoryInternalRecord { Name = actualName, NameHash = GetHash(actualName) });
            return true;
        }

        private TerritoryInternalRecord ExistingRecord(string name) {
            TerritoryInternalRecord existing = null;
            var hash = GetHash(name);
            try {
                existing = _territoryInternalRecord
                    .Table.Where(x => x.NameHash == hash)
                    .FirstOrDefault(x => x.Name == name);
            } catch (Exception) {
                //sqlCE doe not support using strings properly when their length is such that the column
                //in the record is of type ntext.
                var tirs = _territoryInternalRecord
                    .Fetch(x => x.NameHash == hash);
                existing = tirs.ToList().Where(rr => rr.Name == name).FirstOrDefault();
            }
            return existing;
        }

        public TerritoryInternalRecord Update(TerritoryInternalRecord tir) {
            ValidateTir(tir);
            tir.Name = tir.Name.Trim();
            if (GetSameNameIds(tir).Any()) {
                throw new TerritoryInternalDuplicateException(T("A territory with the same name already exists."));
            }
            _territoryInternalRecord.Update(tir);
            return tir.CreateSafeDuplicate();
        }

        public void Delete(int id) {
            var tir = _territoryInternalRecord.Get(id);
            if (tir != null) {
                // Handle connected TerritoryParts
                foreach (var tpr in tir.TerritoryParts) {
                    tpr.TerritoryInternalRecord = null;
                }
                // Delete record
                _territoryInternalRecord.Delete(tir);
            }
        }

        public void Delete(TerritoryInternalRecord tir) {
            if (tir != null) {
                Delete(tir.Id);
            }
        }

        private IEnumerable<int> GetSameNameIds(TerritoryInternalRecord tir) {
            var name = tir.Name.Trim();
            var hash = tir.NameHash;
            try {
                return _territoryInternalRecord.Table
                    .Where(x => x.NameHash == hash 
                        && (tir.Id == 0 || tir.Id != x.Id)) //can have same name as its own self
                    .ToList() //force execution of the query so it can fail in sqlCE
                    .Where(x => x.Name == name)
                    .Select(x => x.Id);
            } catch (Exception) {
                //sqlCE doe not support using strings properly when their length is such that the column
                //in the record is of type ntext.
                var tirs = _territoryInternalRecord
                    .Fetch(x => x.NameHash == hash);
                return tirs
                    .ToList() //force execution so that Linq happens on in-memory objects
                    .Where(x => x.Name == name && (tir.Id == 0 || tir.Id != x.Id))
                    .Select(x => x.Id);
            }
        }

        /// <summary>
        /// Validates a TerritoryInternalRecord parameter, throwing ArgumentExceptions if it fails.
        /// </summary>
        /// <param name="tir">A TerritoryInternalRecord to validate.</param>
        private void ValidateTir(TerritoryInternalRecord tir) {
            if (tir == null) {
                throw new ArgumentNullException("TerritoryInternalRecord");
            }
            if (string.IsNullOrWhiteSpace(tir.Name)) {
                throw new ArgumentNullException("Name");
            }
        }

        private string GetHash(string input) {
            return TerritoriesUtilities.GetHash(_hashAlgorithm, input);
        }

    }
}
