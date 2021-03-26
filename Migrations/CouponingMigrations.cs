using Nwazet.Commerce.Models;
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

        public int UpdateFrom1() {

            SchemaBuilder
                .CreateTable("CouponUsedRecord", table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<int>("CouponRecord_Id")
                    .Column<int>("UserPartRecord_Id")
                    .Column<int>("OrderPartRecord_Id")
                    .Column<DateTime>("DateTimeUTC")
                    .Column<string>("AdditionalUserIdentifier")
                    .Column<string>("IdentifierType"))
                // indexes on foreign keys
                .AlterTable("CouponUsedRecord", table => {
                    table.CreateIndex($"IDX_{nameof(CouponUsedRecord.CouponRecord_Id)}",
                        $"{nameof(CouponUsedRecord.CouponRecord_Id)}");
                    table.CreateIndex($"IDX_{nameof(CouponUsedRecord.UserPartRecord_Id)}",
                        $"{nameof(CouponUsedRecord.UserPartRecord_Id)}");
                    table.CreateIndex($"IDX_{nameof(CouponUsedRecord.OrderPartRecord_Id)}",
                        $"{nameof(CouponUsedRecord.OrderPartRecord_Id)}");
                })
                ;
            return 2;
        }

        public int UpdateFrom2() {

            SchemaBuilder.CreateTable("CouponApplicabilityCriterionRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<string>("Type")
                    .Column<string>("Description")
                    .Column<string>("State", c => c.Unlimited())
                    .Column<string>("Category")
                    .Column<int>("CouponRecord_id")
                );

            return 3;
        }

        public int UpdateFrom3() {

            SchemaBuilder.AlterTable("CouponUsedRecord", table =>
                table.AddColumn<bool>("WasInvalid", c => c.NotNull().WithDefault(false)));

            return 4;
        }

        public int UpdateFrom4() {

            SchemaBuilder.CreateTable("CouponLineCriterionRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<string>("Type")
                    .Column<string>("Description")
                    .Column<string>("State", c => c.Unlimited())
                    .Column<string>("Category")
                    .Column<int>("CouponRecord_id")
                );

            return 5;
        }
    }
}
