using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [OrchardFeature("Nwazet.Commerce")]

    public class ProductExecutionContext {
        public ContentItem ProductItem { get; set; }
        public LocalizedString Message { get; set; }
        public IEnumerable<Permission> AdditionalPermissions { get; set; }
        public Func<ContentItem, ActionResult> ExecutionAction { get; set; }

        public ProductExecutionContext()
        {
            AdditionalPermissions = Enumerable.Empty<Permission>();
        }
    }
}
