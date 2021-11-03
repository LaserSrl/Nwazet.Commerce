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
                .Column<int>("CombinationContainerPartRecord_Id")
                .Column<string>("ProductAttributeValues", column => column.Unlimited())
            );
            SchemaBuilder.CreateTable("CombinationContainerPartRecord", table => table
                .ContentPartRecord()
            );

            ContentDefinitionManager.AlterPartDefinition(
                "CombinationContainerPart", builder => builder
                 .Attachable()
                 .WithDescription("Allows configuring variants of this product based on combinations of attributes."));

            ContentDefinitionManager.AlterPartDefinition(
                "CombinationPart", builder => builder
                 .Attachable()
                 .WithDescription("Represent a variant of a product based on a combination of attributes."));

            return 1;
        }
    }
}
