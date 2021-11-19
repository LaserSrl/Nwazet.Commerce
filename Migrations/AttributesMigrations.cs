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
using Orchard.Data;

namespace Nwazet.Commerce.Migrations {
    [OrchardFeature("Nwazet.Attributes")]
    public class AttributesMigrations : DataMigrationImpl {

        private readonly IContentManager _contentManager;
        private readonly IRepository<ProductAttributeValueRecord> _productAttributeValueRepository;

        public AttributesMigrations(
            IContentManager contentManager,
            IRepository<ProductAttributeValueRecord> productAttributeValueRepository) {
            _contentManager = contentManager;
            _productAttributeValueRepository = productAttributeValueRepository;
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

        public int UpdateFrom6() {
            SchemaBuilder.CreateTable("ProductAttributeValueRecord", table => table
             .Column<int>("Id", col => col.PrimaryKey().Identity())
             .Column<string>("Text", col => col.WithLength(500))
             .Column<decimal>("PriceAdjustment")
             .Column<bool>("IsLineAdjustment", col => col.WithDefault(false))
             .Column<int>("SortOrder")
             .Column<string>("ExtensionProvider", col => col.WithLength(500))
             .Column<int>("ProductAttributeId")
            );

            // merged existing values
            var productAttributeRecords = _contentManager.Query<ProductAttributePart, ProductAttributePartRecord>().List();
            foreach (var attribute in productAttributeRecords) {
                string attributeValue = attribute.AttributeValuesString;
                if (!string.IsNullOrWhiteSpace(attributeValue)) {
                    var itemsAttribute = attributeValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Split('='));
                    foreach (var attr in itemsAttribute) {
                        var attrSettings = attr[1].Split(',');
                        _productAttributeValueRepository.Create(new ProductAttributeValueRecord {
                            Text = attr[0],
                            PriceAdjustment = Convert.ToDecimal(attrSettings[0]),
                            IsLineAdjustment = Convert.ToBoolean(attrSettings[1]),
                            // Check if sort order value is present, didn't exist in previous versions
                            SortOrder = attrSettings.Length > 2 ? Convert.ToInt32(attrSettings[2]) : 0,
                            // Check if extension provider value is present, didn't exist in previous versions
                            ExtensionProvider = attrSettings.Length > 3 ? attrSettings[3] : string.Empty,
                            ProductAttributeId = attribute.Id
                        });
                    }    
                }
            }
            return 7;
        }

        private static string ConvertSerializedAttributeValues(string values) {
            var newValues = values
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a + "=0,False");
            return string.Join(";", newValues);
        }
    }
}
