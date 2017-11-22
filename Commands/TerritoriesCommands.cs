using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Commands {
    [OrchardFeature("Territories")]
    public class TerritoriesCommands : DefaultOrchardCommandHandler {

        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public TerritoriesCommands(
            ITerritoriesRepositoryService territoriesRepositoryService) {
            
            _territoriesRepositoryService = territoriesRepositoryService;
        }

        [CommandName("territories import")]
        [CommandHelp("territories import <name>\r\n\t" + "Creates the TerritoriyInternalRecord with the given name.")]
        public void Import(string territoryName) {
            _territoriesRepositoryService.AddTerritory(new TerritoryInternalRecord { Name = territoryName });
        }
    }
}
