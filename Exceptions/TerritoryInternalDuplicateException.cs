﻿using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Exceptions {
    public class TerritoryInternalDuplicateException : Exception {
        public TerritoryInternalDuplicateException() :
            base("A territory with the same name already exists.") { }
        public TerritoryInternalDuplicateException(LocalizedString message) : base(message.Text) { }
        public TerritoryInternalDuplicateException(string message) : base(message) { }
        public TerritoryInternalDuplicateException(string message, Exception innerException) : base (message, innerException) { }
    }
}
