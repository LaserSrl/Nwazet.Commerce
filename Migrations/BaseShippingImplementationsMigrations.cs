using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.BaseShippingImplementations")]
    public class BaseShippingImplementationsMigrations : DataMigrationImpl {

        public int Create() {
            // we could be calling this because we enabled the feature in the migrations for
            // Nwazet.Shipping. In that case, we do not have to recreate the tables, because
            // those were handled there already.

            try {
                // if the tables exists, this call succeeds. Otherwise it throws an exception.
                SchemaBuilder.ExecuteSql("SELECT * FROM Nwazet_Commerce_WeightBasedShippingMethodPartRecord");
            } catch (Exception) {
                // here we should create the tables.
                SchemaBuilder.CreateTable("WeightBasedShippingMethodPartRecord", table => table
                    .ContentPartRecord()
                    .Column("Name", DbType.String)
                    .Column("ShippingCompany", DbType.String)
                    .Column("Price", DbType.Decimal)
                    .Column("MinimumWeight", DbType.Double, column => column.Nullable())
                    .Column("MaximumWeight", DbType.Double, column => column.Nullable())
                    .Column("IncludedShippingAreas", DbType.String)
                    .Column("ExcludedShippingAreas", DbType.String)
                );
                SchemaBuilder.CreateTable("SizeBasedShippingMethodPartRecord", table => table
                    .ContentPartRecord()
                    .Column("Name", DbType.String)
                    .Column("ShippingCompany", DbType.String)
                    .Column("Price", DbType.Decimal)
                    .Column("Size", DbType.String)
                    .Column("Priority", DbType.Int32)
                    .Column("IncludedShippingAreas", DbType.String)
                    .Column("ExcludedShippingAreas", DbType.String)
                );
            }

            // changes to content definitions can happen here, because it isn't a problem even
            // if they ran already
            ContentDefinitionManager.AlterTypeDefinition("WeightBasedShippingMethod", cfg => cfg
              .WithPart("WeightBasedShippingMethodPart")
              .WithPart("TitlePart"));
            ContentDefinitionManager.AlterTypeDefinition("SizeBasedShippingMethod", cfg => cfg
              .WithPart("SizeBasedShippingMethodPart")
              .WithPart("TitlePart"));

            return 1;
        }
    }
}
