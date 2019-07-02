using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPartDriver : ContentPartDriver<FlexibleShippingMethodPart> {

        private readonly IEnumerable<IShippingAreaProvider> _shippingAreaProviders;
        private readonly IFlexibleShippingManager _flexibleShippingManager;
        private readonly IFormManager _formManager;

        public FlexibleShippingMethodPartDriver(
            IEnumerable<IShippingAreaProvider> shippingAreaProviders,
            IFlexibleShippingManager flexibleShippingManager,
            IFormManager formManager) {

            _shippingAreaProviders = shippingAreaProviders;
            _flexibleShippingManager = flexibleShippingManager;
            _formManager = formManager;
        }

        protected override string Prefix {
            get { return "FlexibleShippingMethodPart"; }
        }

        //GET
        protected override DriverResult Editor(
            FlexibleShippingMethodPart part, dynamic shapeHelper) {

            var shapes = new List<DriverResult>(2);

            // Load applicability criteria if we are not in a new ContentItem
            if (part.Id == 0
                || part.ApplicabilityCriteria == null) {
                shapes.Add(ContentShape("Parts_FlexibleShippingMethod_EditCriteria",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/NewFlexibleShippingMethodCriteria",
                        Prefix: Prefix)));
            } else {
                var criterionEntries = new List<ApplicabilityCriterionEntry>();
                foreach (var criterion in part.ApplicabilityCriteria) {
                    var crit = _flexibleShippingManager
                        .GetCriteria(criterion.Category, criterion.Type);
                    if (crit != null) {
                        criterionEntries.Add(
                            new ApplicabilityCriterionEntry {
                                Category = crit.Category,
                                Type = crit.Type,
                                CriterionRecordId = criterion.Id,
                                DisplayText = string.IsNullOrWhiteSpace(criterion.Description)
                                    ? crit.Display(new CriterionContext {
                                        State = FormParametersHelper.ToDynamic(criterion.State)
                                    }).Text
                                    : criterion.Description
                            });
                    }
                }
                shapes.Add(ContentShape("Parts_FlexibleShippingMethod_EditCriteria",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/FlexibleShippingMethodCriteria",
                        Model: shapeHelper.ShippingEditor(
                            ShippingMethod: part,
                            Criterias: criterionEntries,
                            Prefix: Prefix),
                        Prefix: Prefix)));
            }

            shapes.Add(ContentShape("Parts_FlexibleShippingMethod_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/FlexibleShippingMethod",
                    Model: shapeHelper.ShippingEditor(
                        ShippingMethod: part,
                        ShippingAreas: _shippingAreaProviders.SelectMany(ap => ap.GetAreas()),
                        Prefix: Prefix),
                    Prefix: Prefix)));


            return Combined(shapes.ToArray());
        }

        private class LocalViewModel {
            public string[] IncludedShippingAreas { get; set; }
            public string[] ExcludedShippingAreas { get; set; }
        }

        //POST
        protected override DriverResult Editor(
            FlexibleShippingMethodPart part,
            IUpdateModel updater,
            dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, new[] { "IncludedShippingAreas", "ExcludedShippingAreas" });
            var dyn = new LocalViewModel();
            updater.TryUpdateModel(dyn, Prefix, new[] { "IncludedShippingAreas", "ExcludedShippingAreas" }, null);
            part.IncludedShippingAreas = dyn.IncludedShippingAreas == null
                ? ""
                : string.Join(",", dyn.IncludedShippingAreas);
            part.ExcludedShippingAreas = dyn.ExcludedShippingAreas == null
                ? ""
                : string.Join(",", dyn.ExcludedShippingAreas);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(FlexibleShippingMethodPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element
                .With(part)
                .ToAttr(p => p.Name)
                .ToAttr(p => p.ShippingCompany)
                .ToAttr(p => p.IncludedShippingAreas)
                .ToAttr(p => p.ExcludedShippingAreas)
                .ToAttr(p => p.DefaultPrice);
            // ApplicabilityCriteria
            if (part.ApplicabilityCriteria != null && part.ApplicabilityCriteria.Any()) {
                element.Add(new XElement("ApplicabilityCriteria",
                    part.ApplicabilityCriteria.Select(criterion => {
                        return new XElement("Criterion",
                            new XAttribute("Category", criterion.Category),
                            new XAttribute("Description", criterion.Description),
                            new XAttribute("Type", criterion.Type),
                            new XAttribute("State", criterion.State));
                    })));
            }
        }

        protected override void Importing(FlexibleShippingMethodPart part, ImportContentContext context) {
            var element = context.Data.Element(part.PartDefinition.Name);
            if (element == null) {
                return;
            }
            element
               .With(part)
               .FromAttr(p => p.Name)
               .FromAttr(p => p.ShippingCompany)
               .FromAttr(p => p.IncludedShippingAreas)
               .FromAttr(p => p.ExcludedShippingAreas)
               .FromAttr(p => p.DefaultPrice);
            // ApplicabilityCriteria
            part.Record.ApplicabilityCriteria.Clear();
            foreach (var item in element
                .Element("ApplicabilityCriteria")
                .Elements("Criterion")
                .Select(criterion => {
                    var category = criterion.Attribute("Category").Value;
                    var description = criterion.Attribute("Description").Value;
                    var type = criterion.Attribute("Type").Value;
                    var state = criterion.Attribute("State").Value;
                    var descriptor = _flexibleShippingManager
                        .GetCriteria(category, type);
                    if (descriptor != null) {
                        state = _formManager.Import(descriptor.Form, state, context);
                    }
                    return new ApplicabilityCriterionRecord {
                        Category = category,
                        Description = description,
                        Type = type,
                        State = state
                    };
                })) {
                part.Record.ApplicabilityCriteria.Add(item);
            }
        }
    }
}
