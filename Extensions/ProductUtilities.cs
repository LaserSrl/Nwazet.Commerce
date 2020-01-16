using Orchard.Localization;
using Orchard.Security.Permissions;

namespace Nwazet.Commerce.Extensions {
    public static class ProductUtilities {
        static Localizer T = NullLocalizer.Instance;

        //Messages that are displayed in different Unauthorized conditions
        public static string Default401ProductMessage = T("Not authorized to manage products.").Text;
        public static LocalizedString Creation401ProductMessage = T("Couldn't create product");
        public static LocalizedString Edit401ProductMessage = T("Couldn't edit product");
        public static LocalizedString Delete401ProductMessage = T("Couldn't delete product");

        public static string SpecificProduct401Message(string typeName)
        {
            return T("Not authorized to manage products of type \"{0}\"", typeName).Text;
        }
    }
}
