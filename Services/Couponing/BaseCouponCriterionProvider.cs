using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Localization;

namespace Nwazet.Commerce.Services.Couponing {
    public abstract class BaseCouponCriterionProvider : ICouponCriterionProvider {

        public abstract string ProviderName { get; }
        public abstract LocalizedString ProviderDisplayName { get; }
        
        public virtual bool IsAvailableForConfiguration() {
            // settings part should be cached to prevent repeated fetches of the Site ContentItem
            return true;
        }

        public virtual bool IsAvailableForProcessing() {
            return true;
        }
    }
}
