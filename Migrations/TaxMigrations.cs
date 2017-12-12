using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Records;
using Orchard.Environment.Extensions;
using Orchard.Modules.Services;
using System.Data;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.Taxes")]
    public class TaxMigrations : DataMigrationImpl {

        private readonly IRepository<DataMigrationRecord> _dataMigrationRepository;
        private readonly IModuleService _moduleService;
        private readonly ITransactionManager _transactionManager;

        public TaxMigrations(
            IRepository<DataMigrationRecord> dataMigrationRepository,
            IModuleService moduleService,
            ITransactionManager transactionManager) {

            _dataMigrationRepository = dataMigrationRepository;
            _moduleService = moduleService;
            _transactionManager = transactionManager;
        }

        public int Create() {
            // Update: we moved the StateCountryTax and ZipCodeTax implementations to a separate feature.
            // Hence we need to skip the migrations steps related to them.
            //return 4; //commented for test

            SchemaBuilder.CreateTable("StateOrCountryTaxPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("State")
                .Column<string>("Country")
                .Column<double>("Rate")
                .Column<int>("Priority")
            );

            ContentDefinitionManager.AlterTypeDefinition("StateOrCountryTax", cfg => cfg
              .WithPart("StateOrCountryTaxPart"));

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("ZipCodeTax", cfg => cfg
                .WithPart("ZipCodeTaxPart"));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("StateOrCountryTaxPartRecord", table =>
                table.AlterColumn("Rate", column =>
                    column.WithType(DbType.Decimal)));
            return 3;
        }

        public int UpdateFrom3() {
            // Everything that was done above now belongs to the Nwazet.BaseTaxImplementations feature.
            // If we are running this method, it means we are updating an old pre-territories system.
            // We need to activate the Nwazet.BaseTaxImplementations feature, and not run its migrations,
            // because those would break the db by trying to recreate tables.

            _dataMigrationRepository.Create(new DataMigrationRecord {
                Version = 3,
                DataMigrationClass = "Nwazet.BaseTaxImplementations"
            });

            _transactionManager.RequireNew();

            _moduleService.EnableFeatures(new string[] { "Nwazet.BaseTaxImplementations" }, true);

            return 4;
        }
    }
}
