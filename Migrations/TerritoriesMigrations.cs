using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Territories")]
    public class TerritoriesMigrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("TerritoryInternalRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<string>("Name"));

            SchemaBuilder.CreateTable("TerritoryHierarchyPartRecord", table => table
                .ContentPartRecord());

            SchemaBuilder.CreateTable("TerritoryPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("TerritoryInternalRecord_Id")
                .Column<int>("ParentTerritory_Id")
                .Column<int>("Hierarchy_Id"));
            
            ContentDefinitionManager.AlterPartDefinition(TerritoryHierarchyPart.PartName, builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("TerritoryHierarchy", cfg => cfg
                .WithIdentity()
                .WithPart("TitlePart")
                .WithPart(TerritoryHierarchyPart.PartName)
                .DisplayedAs("Territory Hierarchy"));

            ContentDefinitionManager.AlterTypeDefinition("Territory", cfg => cfg
                .WithIdentity()
                .WithPart("TitlePart"));

            return 1;
        }
    }
}
