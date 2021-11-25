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
                        AttributeValues = part.AttributeValues,
                        AttributeExtensionProviders = _attributeExtensionProviders,
                        CssName = part.CssName,
                        Meaning = part.Meaning,
                        AttributeValueRecords = part.AttributeValueRecords.Select(r => new ProductAttributeValueViewModel() { AttributeValueRecord = r }).ToList()
                    }));
        }

        //POST
        protected override DriverResult Editor(ProductAttributePart part, IUpdateModel updater, dynamic shapeHelper) {
            var technicalName = part.TechnicalName;

            var viewModel = new ProductAttributePartEditViewModel {
                DisplayName = part.DisplayName,
                TechnicalName = part.TechnicalName,
                SortOrder = part.SortOrder,
                AttributeValues = part.AttributeValues,
                AttributeExtensionProviders = _attributeExtensionProviders,
                CssName = part.CssName,
                Meaning = part.Meaning,
                AttributeValueRecords = part.AttributeValueRecords.Select(r => new ProductAttributeValueViewModel() { AttributeValueRecord = r }).ToList()
            };
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.DisplayName = viewModel.DisplayName;
                part.TechnicalName = viewModel.TechnicalName;
                part.SortOrder = viewModel.SortOrder;
                part.AttributeValues = viewModel.AttributeValues;
                part.CssName = viewModel.CssName;
                part.Meaning = viewModel.Meaning;
                foreach (var rec in viewModel.AttributeValueRecords.Where(vm => !vm.Deleted).Select(vm => vm.AttributeValueRecord)) {
                    if (rec.Id==-1) {
                        part.AttributeValueRecords.Add(new ProductAttributeValueRecord {
                            SortOrder = rec.SortOrder,
                            Text = rec.Text,
                            PriceAdjustment = rec.PriceAdjustment,
                            IsLineAdjustment = rec.IsLineAdjustment,
                            ExtensionProvider = rec.ExtensionProvider
                        });
                    } else {
                        var valueRecord = part.AttributeValueRecords
                          .FirstOrDefault(r => r.Id == rec.Id);
                        if (valueRecord != null) {
                            valueRecord.SortOrder = rec.SortOrder;
                            valueRecord.Text = rec.Text;
                            valueRecord.PriceAdjustment = rec.PriceAdjustment;
                            valueRecord.IsLineAdjustment = rec.IsLineAdjustment;
                            valueRecord.ExtensionProvider = rec.ExtensionProvider;
                        }
                    }
                }
                foreach (var rec in viewModel.AttributeValueRecords.Where(vm => vm.Deleted).Select(vm => vm.AttributeValueRecord)) {
                    var valueRecord = part.AttributeValueRecords
                         .FirstOrDefault(r => r.Id == rec.Id);
                    if (valueRecord != null) {
                        part.AttributeValueRecords.Remove(valueRecord);
                    }
                }

                //check TechnicalName for invalid characters
                if (!String.Equals(part.TechnicalName, part.TechnicalName.ToSafeName(), StringComparison.OrdinalIgnoreCase)) {
                    updater.AddModelError("Name", T("The technical name contains invalid characters."));
                }
                //ensure uniqueness of TechnicalName
                var tName = part.TechnicalName;
                var processTechnicalName = _productAttributeNameService.ProcessTechnicalName(part);
                if (!processTechnicalName) {
                    Services.Notifier.Warning(
                        T("Attribute technical names in conflict. \"{0}\" is already set for a previously created attribute so now it has been changed to \"{1}\"",
                        tName, part.TechnicalName));
                }

                // valid CssName and Meaning
                var pattern = @"^[a-zA-Z0-9 ]*$";
                if (!Regex.IsMatch(part.CssName, pattern)) {
                    updater.AddModelError("CssName", T("The css name contains invalid characters."));
                }
                if (!Regex.IsMatch(part.Meaning, pattern)) {
                    updater.AddModelError("Meaning", T("The meaning contains invalid characters."));
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
                //part.Record.AttributeValues = values;
                try {
                    part.AttributeValues = ProductAttributeValue.DeserializeAttributeValues(values);
                } catch (Exception) {

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
            context.Element(part.PartDefinition.Name).SetAttributeValue("Values", part.Record.AttributeValues);
        }
    }
}
