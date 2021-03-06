﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Environment.Extensions;

namespace Nwazet.Commerce.Settings {
    [OrchardFeature("Nwazet.AttributesLocalizationExtension")]
    public class ProductAttributeLocalizationSettings {
        //this settings will be attached to a ProductAttributesPart
        //if the ContentItem has a LocalizationPart

        public ProductAttributeLocalizationSettings() {
            TryToLocalizeAttributes = true;
        }
        public bool TryToLocalizeAttributes { get; set; }
        public bool RemoveAttributesWithoutLocalization { get; set; }
        public bool AssertAttributesHaveSameCulture { get; set; }
        public bool HideAttributesFromEditor { get; set; }
    }
}
