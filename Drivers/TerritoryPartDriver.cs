using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Orchard.ContentManagement;
using Orchard;
using Nwazet.Commerce.Services;
using Orchard.UI.Notify;
using Orchard.Localization;

namespace Nwazet.Commerce.Drivers {
    [OrchardFeature("Territories")]
    public class TerritoryPartDriver : ContentPartDriver<TerritoryPart> {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ITerritoriesService _territoriesService;
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;

        public TerritoryPartDriver(
            IWorkContextAccessor workContextAccessor,
            ITerritoriesService territoriesService,
            INotifier notifier,
            IContentManager contentManager) {

            _workContextAccessor = workContextAccessor;
            _territoriesService = territoriesService;
            _notifier = notifier;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        protected override string Prefix {
            get { return "TerritoryPart"; }
        }

        protected override DriverResult Editor(TerritoryPart part, dynamic shapeHelper) {
            // one of the first things we need to know is what are the TerritoryInternalRecords that 
            // we are allowed to use here.
            int hierarchyId;
            //part.id == 0: new item
            if (part.Id == 0) {
                // we don't know the Hierarchy for this territory here, so we try to get it from
                // the request path (in case we are creating a territory from the HierarchyTerritoriesAdminController
                if (!TryValidateCreationContext(out hierarchyId)) {
                    hierarchyId = 0;
                }
            } else {
                hierarchyId = part.Record.Hierarchy.Id;
            }
            var shapes = new List<DriverResult>();

            if (hierarchyId == 0) {
                // no valid hierarchy means we cannot have a valid list of allowed
                // TerritoryInternalRecord
            } else {

            }

            return Combined(shapes.ToArray());
        }

        protected override DriverResult Editor(TerritoryPart part, IUpdateModel updater, dynamic shapeHelper) {
            return null;
        }

        private bool TryValidateCreationContext(out int hierarchyId) {
            hierarchyId = 0;
            var request = _workContextAccessor.GetContext()
                    .HttpContext.Request;
            var routeValues = request.RequestContext.RouteData.Values;
            if (routeValues.ContainsKey("area") &&
                routeValues.ContainsKey("controller") &&
                routeValues.ContainsKey("action") &&
                routeValues["area"].ToString().Equals("Nwazet.Commerce", StringComparison.OrdinalIgnoreCase) &&
                routeValues["controller"].ToString().Equals("HierarchyTerritoriesAdmin", StringComparison.OrdinalIgnoreCase) &&
                routeValues["action"].ToString().Equals("CreateTerritory", StringComparison.OrdinalIgnoreCase) &&
                request.QueryString.AllKeys.Contains("hierarchyId")) {

                if (int.TryParse(request.QueryString["hierarchyId"], out hierarchyId)) {
                    return true;
                }
            }

            return false;
        }
    }
}
