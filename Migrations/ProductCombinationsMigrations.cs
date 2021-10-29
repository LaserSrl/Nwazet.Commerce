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
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ProductCombinationsMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("CombinationPartRecord", table => table
                .ContentPartRecord()
            );
            SchemaBuilder.CreateTable("CombinationContainerPartRecord", table => table
                .ContentPartRecord()
            );

            ContentDefinitionManager.AlterPartDefinition(
                "CombinationContainerPart", builder => builder
                 .Attachable());

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("CombinationPartRecord", table => table
                .AddColumn<int>("CombinationContainerPartRecord_Id")
            );

            return 2;
        }
    }
}
