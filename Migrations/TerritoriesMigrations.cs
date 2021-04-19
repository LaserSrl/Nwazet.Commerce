using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Territories")]
    public class TerritoriesMigrations : DataMigrationImpl {
        private readonly IRepository<TerritoryInternalRecord> _territoryInternalRecord;
        private readonly IRepository<TerritoryPartRecord> _territoryPartRecord;
        private readonly ITransactionManager _transactionManager;
        public TerritoriesMigrations(
            IRepository<TerritoryInternalRecord> territoryInternalRecord,
            IRepository<TerritoryPartRecord> territoryPartRecord,
            ITransactionManager transactionManager) {

            _territoryInternalRecord = territoryInternalRecord;
            _territoryPartRecord = territoryPartRecord;
            _transactionManager = transactionManager;
        }

        public int Create() {

            SchemaBuilder.CreateTable("TerritoryInternalRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<string>("NameHash")
                .Column<string>("Name", col => col.NotNull().Unlimited()));

            SchemaBuilder.CreateTable("TerritoryHierarchyPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TerritoryType"));

            SchemaBuilder.CreateTable("TerritoryPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("TerritoryInternalRecord_Id")
                .Column<int>("ParentTerritory_Id")
                .Column<int>("Hierarchy_Id")
                .Column<string>("TerritoriesFullPath"));

            // create an index on the name because we often search using that
            // Since the Name column is nvarchar, it cannot have an index itself
            // we need to create a "dependent" column with its checksum and build
            // an index on that. Then we will want to search on that column to
            // take advantage of the index.
            SchemaBuilder.AlterTable("TerritoryInternalRecord", table => table
                .CreateIndex("IX_NameHash", "NameHash"));
            SchemaBuilder.AlterTable("TerritoryPartRecord", table => table
                .CreateIndex("IX_TerritoriesFullPath", "TerritoriesFullPath"));


            // here we have to actually update the hashes in the new column for
            // pre-existing records
            using (HashAlgorithm algo = TerritoriesUtilities.GetHashAlgorithm()) {
                foreach (var tir in _territoryInternalRecord.Table) {
                    tir.NameHash = TerritoriesUtilities.GetHash(algo, tir.Name);
                    _territoryInternalRecord.Update(tir);
                }
            }

            ContentDefinitionManager.AlterPartDefinition(TerritoryHierarchyPart.PartName, builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition(TerritoryPart.PartName, builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("TerritoryHierarchy", typeBuilder => {
                typeBuilder
                    .WithIdentity()
                    .WithPart("TitlePart")
                    .WithPart(TerritoryHierarchyPart.PartName, partBuilder => {
                        partBuilder.WithSetting("TerritoryHierarchyPartSettings.TerritoryType", "Territory");
                    })
                    .DisplayedAs("Territory Hierarchy");

            });

            ContentDefinitionManager.AlterTypeDefinition("Territory", cfg => cfg
                .WithIdentity()
                .WithPart("TitlePart")
                .WithPart(TerritoryPart.PartName));

            return 5;
        }

        public int UpdateFrom1() {
            // create an index on the name because we often search using that
            // Since the Name column is nvarchar, it cannot have an index itself
            // we need to create a "dependent" column with its checksum and build
            // an index on that. Then we will want to search on that column to
            // take advantage of the index.
            SchemaBuilder.AlterTable("TerritoryInternalRecord", table => table
                .AddColumn<string>("NameHash"));
            SchemaBuilder.AlterTable("TerritoryInternalRecord", table => table
                .CreateIndex("IX_NameHash", "NameHash"));
            return 2;
        }
        public int UpdateFrom2() {
            // here we have to actually update the hashes in the new column for
            // pre-existing records
            using (HashAlgorithm algo = TerritoriesUtilities.GetHashAlgorithm()) {
                foreach (var tir in _territoryInternalRecord.Table) {
                    tir.NameHash = TerritoriesUtilities.GetHash(algo, tir.Name);
                    _territoryInternalRecord.Update(tir);
                }
            }
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("TerritoryPartRecord", table => table
                .AddColumn<string>("TerritoriesFullPath"));
            SchemaBuilder.AlterTable("TerritoryPartRecord", table => table
                .CreateIndex("IX_TerritoriesFullPath", "TerritoriesFullPath"));
            return 4;
        }
        public int UpdateFrom4() {
            var territoryItems = _territoryPartRecord.Table.ToList();
            var firstLevel = territoryItems.Where(x => x.ParentTerritory == null || x.ParentTerritory.Id == 0);
            foreach (var item in firstLevel) {
                UpdatePath(0, item.Hierarchy != null ? item.Hierarchy.Id : 0, "\\", territoryItems);
            }
            return 5;
        }

        // this method is same as \Nwazet.Commerce\Commands\TerritoriesCommands.cs
        //TODO: maybe it should be better to have a service for that
        private void UpdatePath(int parentId, int hierarchyId, string parentPath, IEnumerable<TerritoryPartRecord> territoryItems) {
            if (hierarchyId <= 0) {
                return;
            }
            Func<TerritoryPartRecord, bool> condition;
            if (parentId == 0) {
                condition = x => x.ParentTerritory == null || x.ParentTerritory.Id == parentId;
            }
            else {
                condition = x => x.ParentTerritory != null && x.ParentTerritory.Id == parentId;
            }
            if (!territoryItems.Any(condition)) {
                return;
            }
            var current = _transactionManager.GetSession();
            string sqlUpdate;
            if (parentId > 0) {
                sqlUpdate = "UPDATE Nwazet.Commerce.Models.TerritoryPartRecord t SET t.TerritoriesFullPath=concat('" + parentPath.Replace("'", "''") + "', trim(str(t.id)), '\\') WHERE isnull(t.Hierarchy.Id,0)=" + hierarchyId + " and isnull(t.ParentTerritory.Id,0)=" + parentId;
            }
            else {
                sqlUpdate = "UPDATE Nwazet.Commerce.Models.TerritoryPartRecord t SET t.TerritoriesFullPath=concat('" + parentPath.Replace("'", "''") + "', trim(str(t.id)), '\\') WHERE isnull(t.Hierarchy.Id,0)=" + hierarchyId + " and isnull(t.ParentTerritory.Id,0)=0";
            }
            var bulkUpdate = current.CreateQuery(sqlUpdate);
            bulkUpdate.ExecuteUpdate();
            var children = territoryItems.Where(condition);
            foreach (var child in children) {
                if (child.Hierarchy == null) {
                    continue;
                }
                UpdatePath(child.Id, child.Hierarchy.Id, parentPath + child.Id + "\\", territoryItems);
            }
        }
    }
}
