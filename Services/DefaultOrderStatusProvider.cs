using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Services {
    public class DefaultOrderStatusProvider : BaseOrderStatusProvider {

        public DefaultOrderStatusProvider() : base(){ }
        
        public static readonly string[] states = {
            OrderPart.Pending, OrderPart.Accepted, OrderPart.Archived, OrderPart.Cancelled
        };

        public override IEnumerable<string> States => states;

        public override  Dictionary<string, LocalizedString> StatusLabels =>
            new Dictionary<string, LocalizedString> {
                    {OrderPart.Pending, T("Pending")},
                    {OrderPart.Accepted, T("Accepted")},
                    {OrderPart.Archived, T("Archived")},
                    {OrderPart.Cancelled, T("Cancelled")}
                };
    }
}
