using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class AdvancedVATMigrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("TerritoryVatConfigurationPartRecord", table => table
                .ContentPartRecord());

            SchemaBuilder.CreateTable("HierarchyVatConfigurationPartRecord", table => table
                .ContentPartRecord());

            SchemaBuilder.CreateTable("VatConfigurationPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TaxProductCategory", col => col.Unlimited()) // uniqueness on this will have to be enforced by services
                .Column<int>("Priority")
                .Column<decimal>("DefaultRate"));

            SchemaBuilder.CreateTable("HierarchyVatConfigurationIntersectionRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("Hierarchy_Id")
                .Column<int>("VatConfiguration_Id")
                .Column<decimal>("Rate"));
            
            SchemaBuilder.CreateForeignKey(
                "FK_HierarchyVatHierarchy",
                "HierarchyVatConfigurationIntersectionRecord", new[] { "Hierarchy_Id" },
                "HierarchyVatConfigurationPartRecord", new[] { "Id" });

            SchemaBuilder.CreateForeignKey(
                "FK_HierarchyVatVat",
                "HierarchyVatConfigurationIntersectionRecord", new[] { "VatConfiguration_Id" },
                "VatConfigurationPartRecord", new[] { "Id" });

            ContentDefinitionManager.AlterTypeDefinition("VATConfiguration", cfg => cfg
                .WithPart("VatConfigurationPart")
                .DisplayedAs("VAT Category Configuration"));

            return 1;
        }

    }
}
