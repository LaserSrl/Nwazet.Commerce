using Nwazet.Commerce.Models.Couponing;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponingMigrations : DataMigrationImpl {

        public int Create() {
            // this only works if CouponRecord is in the 
            // Nwazet.Commerce.Models namepspace.
            // Using e.g. Nwazet.Commerce.Models.Couponing
            // causes everything to fail down the line.
            SchemaBuilder.CreateTable("CouponRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<string>("Name", col => col.NotNull().Unlimited())
                .Column<string>("Code", col => col.NotNull())
                .Column<bool>("Published")
                .Column<decimal>("Value")
                .Column<string>("CouponType"));

            return 1;
        }
    }
}
