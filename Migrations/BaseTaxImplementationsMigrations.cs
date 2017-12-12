using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Data;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.BaseTaxImplementations")]
    public class BaseTaxImplementationsMigrations : DataMigrationImpl {

        private readonly ITransactionManager _transactionManager;

        public BaseTaxImplementationsMigrations(
            ITransactionManager transactionManager) {

            _transactionManager = transactionManager;
        }

        public int Create() {
            // we could be calling this because we enabled the feature in the migrations for 
            // Nwazet.Taxes. In that case, we do not have to actually create the tables, because
            // those were handled there in the past.
            try {

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
            } catch (Exception ex) {

                //_transactionManager.RequireNew();
                return 3;
            }
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
    }
}
