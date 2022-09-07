using System;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using System.Text.RegularExpressions;
using System.Linq;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.Attributes")]
    public class ProductAttributePartDriver : ContentPartDriver<ProductAttributePart> {

        private readonly IEnumerable<IProductAttributeExtensionProvider> _attributeExtensionProviders;
        private readonly IProductAttributeNameService _productAttributeNameService;

        private const string _cssNamePattern = @"-?[_a-zA-Z]+[_a-zA-Z0-9-]*";

        public ProductAttributePartDriver(
           IOrchardServices services,
           IEnumerable<IProductAttributeExtensionProvider> attributeExtensionProviders,
            IProductAttributeNameService productAttributeNameService) {

            Services = services;
            _attributeExtensionProviders = attributeExtensionProviders;
            _productAttributeNameService = productAttributeNameService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        protected override string Prefix { get { return "NwazetCommerceAttribute"; } }

        protected override DriverResult Display(
            ProductAttributePart part, string displayType, dynamic shapeHelper) {
            // The attribute part should never appear on the front-end.
            return null;
        }

        //GET
        protected override DriverResult Editor(ProductAttributePart part, dynamic shapeHelper) {
            return ContentShape(
                "Parts_ProductAttribute_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/ProductAttribute",
                    Prefix: Prefix,
                    Model: new ProductAttributePartEditViewModel {
                        DisplayName = part.DisplayName,
                        TechnicalName = part.TechnicalName,
                        SortOrder = part.SortOrder,
                        AttributeExtensionProviders = _attributeExtensionProviders,
                        CssName = part.CssName,
                        Meaning = part.Meaning,
                        AttributeValueRecords = part.Record.AttributeValueRecords
                            .OrderBy(r => r.SortOrder)
                            .Select(r => new ProductAttributeValueViewModel() { AttributeValueRecord = r })
                            .ToList()                            
                    }));
        }

        //POST
        protected override DriverResult Editor(ProductAttributePart part, IUpdateModel updater, dynamic shapeHelper) {
            var technicalName = part.TechnicalName;

            var viewModel = new ProductAttributePartEditViewModel {
                DisplayName = part.DisplayName,
                TechnicalName = part.TechnicalName,
                SortOrder = part.SortOrder,
                AttributeExtensionProviders = _attributeExtensionProviders,
                CssName = part.CssName,
                Meaning = part.Meaning,
                AttributeValueRecords = part.Record.AttributeValueRecords
                    .OrderBy(r => r.SortOrder)
                    .Select(r => new ProductAttributeValueViewModel() { AttributeValueRecord = r })
                    .ToList()
            };
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.DisplayName = viewModel.DisplayName?.Trim();
                part.TechnicalName = viewModel.TechnicalName?.Trim();
                part.SortOrder = viewModel.SortOrder;
                part.CssName = viewModel.CssName?.Trim();
                part.Meaning = viewModel.Meaning?.Trim();
                foreach (var rec in viewModel.AttributeValueRecords
                    .Where(vm => !vm.Deleted).Select(vm => vm.AttributeValueRecord)) {
                    // for each of the attribute values that aren't marked to be deleted:
                    if (rec.Id == -1) {
                        // added new product attribute value record
                        part.Record.AttributeValueRecords.Add(new ProductAttributeValueRecord {
                            GUIdentifier = rec.GUIdentifier,
                            SortOrder = rec.SortOrder,
                            Text = rec.Text,
                            PriceAdjustment = rec.PriceAdjustment,
                            IsLineAdjustment = rec.IsLineAdjustment,
                            ExtensionProvider = rec.ExtensionProvider
                        });
                    } else {
                        // updated product attribute value record
                        var valueRecord = part.Record.AttributeValueRecords
                          .FirstOrDefault(r => r.Id == rec.Id);
                        if (valueRecord != null) {
                            valueRecord.GUIdentifier = rec.GUIdentifier;
                            valueRecord.SortOrder = rec.SortOrder;
                            valueRecord.Text = rec.Text;
                            valueRecord.PriceAdjustment = rec.PriceAdjustment;
                            valueRecord.IsLineAdjustment = rec.IsLineAdjustment;
                            valueRecord.ExtensionProvider = rec.ExtensionProvider;
                        }
                    }
                }
                foreach (var rec in viewModel.AttributeValueRecords
                    .Where(vm => vm.Deleted)
                    .Select(vm => vm.AttributeValueRecord)) {
                    // for each of the attribute values that are marked to be deleted:
                    var valueRecord = part.Record.AttributeValueRecords
                         .FirstOrDefault(r => r.Id == rec.Id);
                    if (valueRecord != null) {
                        part.Record.AttributeValueRecords.Remove(valueRecord);
                    }
                }

                // check TechnicalName for invalid characters
                if (!string.Equals(part.TechnicalName, part.TechnicalName.ToSafeName(), StringComparison.OrdinalIgnoreCase)) {
                    updater.AddModelError("Name", T("The technical name contains invalid characters."));
                }
                // ensure uniqueness of TechnicalName
                var tName = part.TechnicalName;
                var processTechnicalName = _productAttributeNameService.ProcessTechnicalName(part);
                if (!processTechnicalName) {
                    Services.Notifier.Warning(
                        T("Attribute technical names in conflict. \"{0}\" is already set for a previously created attribute so now it has been changed to \"{1}\"",
                        tName, part.TechnicalName));
                }

                // validate CssName and Meaning. Both are optional.
                if (!string.IsNullOrWhiteSpace(part.CssName)) {
                    var cssNames = part.CssName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim());
                    if (!cssNames.All(n => Regex.IsMatch(n, _cssNamePattern))) {
                        updater.AddModelError("CssName", T("The css name contains invalid characters."));
                    }
                }
                if (!string.IsNullOrWhiteSpace(part.Meaning)) {
                    if (!Regex.IsMatch(part.Meaning, _cssNamePattern)) {
                        updater.AddModelError("Meaning", T("The meaning contains invalid characters."));
                    }
                }

                // in edit the value of technical name cannot be changed
                if (!string.IsNullOrEmpty(technicalName) && technicalName != part.TechnicalName && processTechnicalName) {
                    part.TechnicalName = technicalName;
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Importing(ProductAttributePart part, ImportContentContext context) {
            var values = context.Attribute(part.PartDefinition.Name, "Values");
            if (!String.IsNullOrWhiteSpace(values)) {
                var attributeValueRecords = ProductAttributeValueRecord.DeserializeAttributeValues(values).ToList();
                foreach (var rec in attributeValueRecords) {
                    var attributeRecord = part.Record.AttributeValueRecords.FirstOrDefault(r => r.GUIdentifier == rec.GUIdentifier);
                    if (attributeRecord != null) {
                        attributeRecord.GUIdentifier = rec.GUIdentifier;
                        attributeRecord.Text = rec.Text;
                        attributeRecord.SortOrder = rec.SortOrder;
                        attributeRecord.PriceAdjustment = rec.PriceAdjustment;
                        attributeRecord.IsLineAdjustment = rec.IsLineAdjustment;
                        attributeRecord.ExtensionProvider = rec.ExtensionProvider;
                    } else {
                        part.Record.AttributeValueRecords.Add(new ProductAttributeValueRecord {
                            GUIdentifier = rec.GUIdentifier,
                            Text = rec.Text,
                            SortOrder = rec.SortOrder,
                            PriceAdjustment = rec.PriceAdjustment,
                            IsLineAdjustment = rec.IsLineAdjustment,
                            ExtensionProvider = rec.ExtensionProvider
                        });
                    }
                }
            } 
            part.DisplayName = context.Attribute(part.PartDefinition.Name, "DisplayName");
            part.TechnicalName = context.Attribute(part.PartDefinition.Name, "TechnicalName");
            part.CssName = context.Attribute(part.PartDefinition.Name, "CssName");
            part.Meaning = context.Attribute(part.PartDefinition.Name, "Meaning");
            int so = 0;
            int.TryParse(context.Attribute(part.PartDefinition.Name, "SortOrder"), out so);
            part.SortOrder = so;
        }

        protected override void Exporting(ProductAttributePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SortOrder", part.SortOrder);
            context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayName", part.DisplayName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("TechnicalName", part.TechnicalName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CssName", part.CssName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Meaning", part.Meaning);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Values", ProductAttributeValueRecord.SerializeAttributeValues(part.Record.AttributeValueRecords));
        }

    }
}
