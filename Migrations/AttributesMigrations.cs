using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Linq;
using Orchard.Utility.Extensions;
using Nwazet.Commerce.Extensions;
using System.Collections.Generic;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.Attributes")]
    public class AttributesMigrations : DataMigrationImpl {

        private readonly IContentManager _contentManager;

        public AttributesMigrations(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public int Create() {
            SchemaBuilder.CreateTable("ProductAttributePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("AttributeValues", col => col.Unlimited())
                .Column<int>("SortOrder", c => c.WithDefault(0))
                .Column<string>("DisplayName")
                .Column<string>("TechnicalName")
                .Column<string>("CssName")
                .Column<string>("Meaning")
            );

            SchemaBuilder.CreateTable("ProductAttributesPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Attributes")
            );

            ContentDefinitionManager.AlterTypeDefinition("ProductAttribute", cfg => cfg
                .WithPart("TitlePart")
                .WithPart("ProductAttributePart")
                .WithPart("IdentityPart"));

            ContentDefinitionManager.AlterTypeDefinition("Product", cfg => cfg
                .WithPart("ProductAttributesPart"));

            return 6;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<int>("SortOrder", c => c.WithDefault(0)));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("DisplayName"));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("TechnicalName"));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("CssName"));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("Meaning"));

            ContentDefinitionManager.AlterTypeDefinition("ProductAttribute", cfg => cfg
                .WithPart("IdentityPart"));

            // Convert existing attribute data to new serlialization format (Attr1/nAttr2/n --> Attr1=0,False;Attr2=0,False)
            var existingAttributeParts = _contentManager.Query<ProductAttributePart>("ProductAttribute").List();
            foreach (var attr in existingAttributeParts) {
                attr.AttributeValuesString = ConvertSerializedAttributeValues(attr.AttributeValuesString);
            }
            // generate technical names for existing attributes
            var partsArray = existingAttributeParts.ToArray();
            for (int i = 0; i < partsArray.Length; i++) {
                partsArray[i].TechnicalName =
                    AttributeNameUtilities.GenerateAttributeTechnicalName(partsArray[i], partsArray.Take(i));
            }
            return 6;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<int>("SortOrder", c => c.WithDefault(0)));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("DisplayName"));
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("TechnicalName"));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("CssName"));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("Meaning"));

            ContentDefinitionManager.AlterTypeDefinition("ProductAttribute", cfg => cfg
                .WithPart("IdentityPart"));

            //generate technical names for existing attributes
            var existingAttributeParts = _contentManager.Query<ProductAttributePart>("ProductAttribute").List().ToArray();
            for (int i = 0; i < existingAttributeParts.Length; i++) {
                existingAttributeParts[i].TechnicalName = 
                    AttributeNameUtilities.GenerateAttributeTechnicalName(existingAttributeParts[i], existingAttributeParts.Take(i));
            }

            return 6;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterTypeDefinition("ProductAttribute", cfg => cfg
                .WithPart("IdentityPart"));

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("CssName"));
            SchemaBuilder.AlterTable("ProductAttributePartRecord", table => table
                .AddColumn<string>("Meaning"));
            return 6;
        }

        private static string ConvertSerializedAttributeValues(string values) {
            var newValues = values
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a + "=0,False");
            return string.Join(";", newValues);
        }
    }
}
