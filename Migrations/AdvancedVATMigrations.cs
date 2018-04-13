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

            SchemaBuilder.CreateTable("VatConfigurationPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TaxProductCategory", col => col.Unlimited()) // uniqueness on this will have to be enforced by services
                .Column<int>("Priority")
                .Column<decimal>("DefaultRate")
                .Column<decimal>("DefaultExtraRate")
                .Column<int>("Hierarchy_Id"));

            ContentDefinitionManager.AlterTypeDefinition("VATConfiguration", cfg => cfg
                .WithPart("VatConfigurationPart")
                .DisplayedAs("VAT Category Configuration"));

            return 1;
        }
        
    }
}
