using System.Data;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using System.Linq;
using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.Commerce")]
    public class CommerceMigrations : DataMigrationImpl {

        private readonly IRepository<ProductPartRecord> _repository;
        IRepository<ProductPartVersionRecord> _versionRepository;
        IRepository<InventoryPartRecord> _inventoryPartRepository;
        private readonly IContentManager _contentManager;

        public CommerceMigrations(
            IRepository<ProductPartRecord> repository,
            IRepository<ProductPartVersionRecord> versionRepository,
            IRepository<InventoryPartRecord> inventoryPartRepository,
            IContentManager contentManager) {

            _repository = repository;
            _versionRepository = versionRepository;
            _inventoryPartRepository = inventoryPartRepository;
            _contentManager = contentManager;
        }

        public int Create() {
            SchemaBuilder.CreateTable("ProductPartRecord", table => table
                .ContentPartRecord()
                .Column("Sku", DbType.String)
                .Column("Price", DbType.Double)
                .Column("ShippingCost", DbType.Single, column => column.Nullable())
                .Column("Weight", DbType.Single)
                .Column("IsDigital", DbType.Boolean, column => column.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition("ProductPart",
              builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("Product", cfg => cfg
              .WithPart("Product")
              .WithPart("CommonPart")
              .WithPart("TitlePart")
              .WithPart("AutoroutePart", builder => builder
                  .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                  .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                  .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-product'}]")
                  .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
              .WithPart("BodyPart")
              .WithPart("ProductPart")
              .WithPart("TagsPart")
              .Creatable()
              .Indexed());

            ContentDefinitionManager.AlterPartDefinition("Product",
                builder => builder
                    .WithField("ProductImage",
                        fieldBuilder => fieldBuilder
                            .OfType("MediaLibraryPickerField")
                            .WithDisplayName("Product Image")));
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition(
                "Product", cfg => cfg.Draftable());
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("ShoppingCartWidget", type => type
                .WithPart("ShoppingCartWidgetPart")
                .WithPart("CommonPart")
                .WithPart("WidgetPart")
                .WithSetting("Stereotype", "Widget")
                );

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<int>("Inventory", c => c.WithDefault(0)));

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<string>("OutOfStockMessage"));
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<bool>("AllowBackOrder", c => c.WithDefault(true)));

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<string>("Size"));
            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<int>("MinimumOrderQuantity"));
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<bool>("AuthenticationRequired"));
            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<bool>("OverrideTieredPricing", c => c.WithDefault(false)));
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<string>("PriceTiers"));
            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.AlterTable("ProductPartRecord", table => table
                .AddColumn<double>("DiscountPrice", column => column
                    .NotNull().WithDefault(-1)));
            return 9;
        }

        public int UpdateFrom9() {
            SchemaBuilder.AlterTable("ProductPartRecord",
            table => table
                .CreateIndex("IDX_ProductPart_Sku", "Sku")
            );
            return 10;
        }

        public int UpdateFrom10() {
            SchemaBuilder.AlterTable("ProductPartRecord", table =>
                table.AddColumn<bool>("ConsiderInventory"));
            return 11;
        }

        public int UpdateFrom11() {
            //Versioning of ProductPart (and we take the chance to upgrade to decimal)
            SchemaBuilder.CreateTable("ProductPartVersionRecord", table => table
                .ContentPartVersionRecord()
                .Column("Sku", DbType.String)
                .Column("Price", DbType.Decimal)
                .Column("ShippingCost", DbType.Decimal, column => column.Nullable())
                .Column("Weight", DbType.Single)
                .Column("IsDigital", DbType.Boolean, column => column.WithDefault(false))
                .Column("Inventory", DbType.Int32, column => column.WithDefault(0))
                .Column("OutOfStockMessage", DbType.String)
                .Column("AllowBackOrder", DbType.Boolean, column => column.WithDefault(false))
                .Column("Size", DbType.String)
                .Column("MinimumOrderQuantity", DbType.Int32)
                .Column("AuthenticationRequired", DbType.Boolean)
                .Column("OverrideTieredPricing", DbType.Boolean, column => column.WithDefault(false))
                .Column("PriceTiers", DbType.String)
                .Column("DiscountPrice", DbType.Decimal, column => column.NotNull().WithDefault(-1))
                .Column("ConsiderInventory", DbType.Boolean)
            );

            foreach (var row in _repository.Table) {
                foreach (var version in row.ContentItemRecord.Versions) {
                    var newItem = new ProductPartVersionRecord() {
                        ContentItemRecord = row.ContentItemRecord,
                        ContentItemVersionRecord = version,
                        Sku = row.Sku,
                        Price = row.Price,
                        ShippingCost = row.ShippingCost,
                        Weight = row.Weight,
                        IsDigital = row.IsDigital,
                        Inventory = row.Inventory,
                        OutOfStockMessage = row.OutOfStockMessage,
                        AllowBackOrder = row.AllowBackOrder,
                        Size = row.Size,
                        MinimumOrderQuantity = row.MinimumOrderQuantity,
                        AuthenticationRequired = row.AuthenticationRequired,
                        OverrideTieredPricing = row.OverrideTieredPricing,
                        PriceTiers = row.PriceTiers,
                        DiscountPrice = row.DiscountPrice,
                        ConsiderInventory = row.ConsiderInventory
                    };
                    _versionRepository.Create(newItem);
                }
            }

            return 12;
        }

        public int UpdateFrom12() {
            // since we moved to ProductPartVersionRecord, the other table is just a waste
            SchemaBuilder.DropTable("ProductPartRecord");
            return 13;
        }

        // THE BIG INVENTORIES OVERHAUL
        public int UpdateFrom13() {
            //PART 1 of THE BIG INVENTORIES OVERHAUL
            // new partrecord to map product inventories that are self-managed by the merchant
            SchemaBuilder.CreateTable("InventoryPartRecord", table => table
               .ContentPartRecord()
               .Column<int>("Inventory")
               .Column<string>("OutOfStockMessage", col => col.Unlimited())
               .Column<bool>("AllowBackOrder")
               .Column<int>("MinimumOrderQuantity"));
            // new part to configure inventories in products
            ContentDefinitionManager.AlterPartDefinition("InventoryPart",
              builder => builder
                .Attachable()
                .WithDescription("This part provides a way to manage inventory for a product. "
                    + "This part allows storage of the portion of inventory that is managed by the merchant themselves."
                    + "It should only be attached to ContentTypes that have a ProductPart. "));

            // Attach the new InventoryPart to all ContentTypes that used to have the
            // ProductPart
            var productTypes = ContentDefinitionManager
                .ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(ctpd => ctpd.PartDefinition.Name.Equals("ProductPart")));
            foreach (var pt in productTypes) {
                ContentDefinitionManager.AlterTypeDefinition(pt.Name, cfg => cfg
                    .WithPart("InventoryPart"));
            }
            // Move values from "old" ProductPart to "new" InventoryPart
            // get ProductPartVersionRecords
            var allProducts = _contentManager
                .Query<ProductPart>(VersionOptions.Latest)
                .List();
            foreach (var product in allProducts) {
                // these products have no InventoryPart yet
                var oldRecord = product.Record;
                var newRecord = new InventoryPartRecord {
                    // pair with ContentItem
                    ContentItemRecord = oldRecord.ContentItemRecord,
                    // copy values
                    Inventory = oldRecord.Inventory,
                    OutOfStockMessage = oldRecord.OutOfStockMessage,
                    AllowBackOrder = oldRecord.AllowBackOrder,
                    MinimumOrderQuantity = oldRecord.MinimumOrderQuantity
                };
                _inventoryPartRepository.Create(newRecord);
            }

            return 14;
        }
        // for PART 2 of THE BIG INVENTORIES OVERHAUL
        // we would like to drop the columns od ProductPartVersionRecord that have become obsolete 
        // thanks to the introduction of InventoryPart.
        // However, since when we created them we added some constraints to them (e.g. default values),
        // we should drop the constraints first (or SQL will fail). Since there is no trivial way
        // to identify those constraints through code, we keep those unused columns for now.

    }
}
