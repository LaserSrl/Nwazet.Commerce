using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.InventoryControl")]
    public class InventoryControlMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("InventoryControlPartRecord", table => table
                .ContentPartRecord()
                .Column("PreventAutomaticDecrease", DbType.Boolean, column => column.WithDefault(false)));

            ContentDefinitionManager.AlterPartDefinition("InventoryControlPart",
                builder => builder.Attachable());

            return 1;
        }

        public int UpdateFrom1() {

            ContentDefinitionManager.AlterPartDefinition("InventoryControlPart",
                builder => builder
                    .WithDescription("Enables automatic decrease of inventory when products are ordered."));

            return 2;
        }
    }
}
