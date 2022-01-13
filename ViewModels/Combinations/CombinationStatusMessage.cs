using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels.Combinations {
    public class CombinationStatusMessage {
        public string Message { get; set; }

        public Severity Severity { get; set; }
    }

    public enum Severity { Error, Warning, Success, Information, }
}
