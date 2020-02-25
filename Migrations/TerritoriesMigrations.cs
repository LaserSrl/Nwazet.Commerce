using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System.Security.Cryptography;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Territories")]
    public class TerritoriesMigrations : DataMigrationImpl {
        private readonly IRepository<TerritoryInternalRecord> _territoryInternalRecord;

        public TerritoriesMigrations(
            IRepository<TerritoryInternalRecord> territoryInternalRecord) {

            _territoryInternalRecord = territoryInternalRecord;
        }

        public int Create() {

            SchemaBuilder.CreateTable("TerritoryInternalRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<string>("Name", col => col.NotNull().Unlimited()));

            SchemaBuilder.CreateTable("TerritoryHierarchyPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TerritoryType"));

            SchemaBuilder.CreateTable("TerritoryPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("TerritoryInternalRecord_Id")
                .Column<int>("ParentTerritory_Id")
                .Column<int>("Hierarchy_Id"));

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

            return 1;
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
                .CreateIndex("IX_NameHash","NameHash"));
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
    }
}
