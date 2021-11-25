using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce {
    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("Nwazet.iframe-transport")
                .SetUrl("jquery.iframe-transport.min.js","jquery.iframe-transport.js").SetDependencies("jQuery"); //Duplicated: should we use Orchard.Resources version?
            manifest.DefineScript("Nwazet.ShoppingCart")
                .SetUrl("shoppingcart.min.js?v=1.2", "shoppingcart.js?v=1.2").SetDependencies("jQuery");
            manifest.DefineScript("Nwazet.CartSpinner")
                .SetUrl("cartspinner.min.js?v=1.2", "cartspinner.js?v=1.2").SetDependencies("jQuery");
            manifest.DefineScript("Nwazet.AttributeExtensions")
                .SetUrl("attribute-extensions.min.js", "attribute-extensions.js");

            manifest.DefineScript("Nwazet.Inventory").SetUrl("inventory.min.js?v=1.0", "inventory.js?v=1.0");
            manifest.DefineScript("Nwazet.Order-Admin").SetUrl("order-admin.min.js", "order-admin.js");
            manifest.DefineScript("Mustache").SetUrl("mustache.min.js", "mustache.js");
            manifest.DefineScript("Nwazet.Territory-Hierarchies-Admin").SetUrl("territory-hierarchies-admin.min.js", "territory-hierarchies-admin.js");
            manifest.DefineScript("Nwazet.Wishlists").SetUrl("wishlists.min.js", "wishlists.js");
            manifest.DefineScript("Nwazet.ChartJs").SetUrl("chartjs.min.js", "chartjs.js");
            manifest.DefineScript("Nwazet.Report").SetUrl("report.min.js", "report.js");
            manifest.DefineScript("Nwazet.Referral").SetUrl("referral.min.js", "referral.js");

            manifest.DefineScript("Stripe").SetUrl("https://js.stripe.com/v1/");
            manifest.DefineScript("Nwazet.Ship").SetUrl("ship.min.js", "ship.js");
            manifest.DefineStyle("Nwazet.Bundle-Admin").SetUrl("bundle.nwazet-commerce-admin.min.css", "bundle.nwazet-commerce-admin.css");
            manifest.DefineStyle("Nwazet.Discount-Admin").SetUrl("discount.nwazet-commerce-admin.min.css", "discount.nwazet-commerce-admin.css");
            manifest.DefineStyle("Nwazet.Order-Admin").SetUrl("order-admin.min.css", "order-admin.css");
            manifest.DefineStyle("Nwazet.Attribute-Admin").SetUrl("attribute.nwazet-commerce-admin.min.css?v=1.1", "attribute.nwazet-commerce-admin.css?v=1.1");
            manifest.DefineStyle("Nwazet.Product-Admin").SetUrl("product-admin.min.css?v=1.1", "product-admin.css");
            manifest.DefineStyle("Nwazet.Coupon-Admin").SetUrl("product-admin.min.css?v=1.0", "product-admin.css");
            manifest.DefineStyle("Nwazet.Report-Admin").SetUrl("reports-admin.min.css", "reports-admin.css");
            manifest.DefineStyle("Nwazet.Territory-Hierarchies-Admin").SetUrl("territory-hierarchies-admin.min.css", "territory-hierarchies-admin.css");
            manifest.DefineStyle("Nwazet.WishLists").SetUrl("wishlists.nwazet-commerce.min.css", "wishlists.nwazet-commerce.css");
            
            manifest.DefineScript("Nwazet.ProductAvailability")
                .SetUrl("productsunavailable.min.js?v=1.0", "productsunavailable.js?v=1.0").SetDependencies("jQuery");

            manifest.DefineScript("Nwazet.ProductCombinations")
                .SetUrl("productcombinations.min.js?v=1.0", "productcombinations.js?v=1.0").SetDependencies("jQuery");
        }
    }
}
