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

            SchemaBuilder.CreateTable("VatConfigurationPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Category", col => col.NotNull().Unlimited().Unique())
                .Column<int>("Priority")
                .Column<bool>("IsDefaultCategory")
                .Column<decimal>("DefaultRate")
                .Column<int>("Hierarchy_Id"));

            return 1;
        }
    }
}
