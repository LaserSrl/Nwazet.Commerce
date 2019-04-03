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

            manifest.DefineScript("Nwazet.iframe-transport").SetUrl("jquery.iframe-transport.js").SetDependencies("jQuery");
            manifest.DefineScript("Nwazet.ShoppingCart")
                .SetUrl("shoppingcart.min.js", "shoppingcart.js").SetDependencies("jQuery");
        }
    }
}
