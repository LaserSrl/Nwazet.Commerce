﻿using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class HierarchyVatConfigurationPartViewModel {

        public VatConfigurationDetailViewModel[] AllVatConfigurations  { get; set; }

    }
    
}
