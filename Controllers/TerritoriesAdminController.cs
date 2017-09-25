using Nwazet.Commerce.Services;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Territories")]
    public class TerritoriesAdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ITerritoriesService _territoriesService;

        public TerritoriesAdminController(
            IOrchardServices orchardServices,
            ITerritoriesService territoriesService) {

            _orchardServices = orchardServices;
            _territoriesService = territoriesService;
        }
    }
}
