using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Modules.Services;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.Shipping")]
    public class ShippingMigrations : DataMigrationImpl {

        private readonly IModuleService _moduleService;

        public ShippingMigrations(
            IModuleService moduleService) {

            _moduleService = moduleService;
        }

        public int Create() {
            // Update: we moved WeightBasedShippingMethod and SizeBasedShippingMethod implementations
            // to the BaseShippingImplementations features, so we need to skip the migration steps
            // related to them here.
            return 4;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("SizeBasedShippingMethodPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShippingCompany", DbType.String)
                .Column("Price", DbType.Double)
                .Column("Size", DbType.String)
                .Column("Priority", DbType.Int32)
                .Column("IncludedShippingAreas", DbType.String)
                .Column("ExcludedShippingAreas", DbType.String)
            );

            ContentDefinitionManager.AlterTypeDefinition("SizeBasedShippingMethod", cfg => cfg
              .WithPart("SizeBasedShippingMethodPart")
              .WithPart("TitlePart"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("WeightBasedShippingMethodPartRecord", table =>
                table.AlterColumn("Price", column =>
                    column.WithType(DbType.Decimal)));
            SchemaBuilder.AlterTable("SizeBasedShippingMethodPartRecord", table =>
                table.AlterColumn("Price", column =>
                    column.WithType(DbType.Decimal)));
            return 3;
        }

        public int UpdateFrom3() {
            // The tables created and updated in the previous steps now belong to the BaseShippingImplementations
            // feature.

            _moduleService.EnableFeatures(new string[] { "Nwazet.BaseShippingImplementations" }, true);

            return 4;
        }
    }
}
