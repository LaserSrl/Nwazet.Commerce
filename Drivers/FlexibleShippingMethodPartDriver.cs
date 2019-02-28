using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPartDriver : ContentPartDriver<FlexibleShippingMethodPart> {

        private readonly IEnumerable<IShippingAreaProvider> _shippingAreaProviders;
        private readonly IFlexibleShippingManager _flexibleShippingManager;

        public FlexibleShippingMethodPartDriver(
            IEnumerable<IShippingAreaProvider> shippingAreaProviders,
            IFlexibleShippingManager flexibleShippingManager) {

            _shippingAreaProviders = shippingAreaProviders;
            _flexibleShippingManager = flexibleShippingManager;
        }

        protected override string Prefix {
            get { return "FlexibleShippingMethodPart"; }
        }

        //GET
        protected override DriverResult Editor(
            FlexibleShippingMethodPart part, dynamic shapeHelper) {

            var shapes = new List<DriverResult>(2);

            // Load applicability criteria if we are not in a new ContentItem
            if (part.Id == 0) {
                shapes.Add(ContentShape("Parts_FlexibleShippingMethod_EditCriteria",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/NewFlexibleShippingMethodCriteria",
                        Prefix: Prefix)));
            } else {
                var allCriteria = _flexibleShippingManager
                    .DescribeCriteria()
                    .SelectMany(x => x.Descriptors)
                    .ToList();
                var criterionEntries = new List<ApplicabilityCriterionEntry>();
                foreach (var criterion in part.ApplicabilityCriteria) {
                    var crit = allCriteria.FirstOrDefault(c => 
                            c.Category == criterion.Category
                            && c.Type == criterion.Type 
                        );
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

            shapes.Add( ContentShape("Parts_FlexibleShippingMethod_Edit",
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
    }
}
