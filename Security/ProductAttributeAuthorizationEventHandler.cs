using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Nwazet.Commerce.Security {
    public class ProductAttributeAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) {
            Permission permission = context.Permission;
            // adjusting permissions only if the content is not securable
            // or is attacchable to CT
            if ((context.Content.Is<ProductAttributePart>()
                && !context.Content.ContentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Securable)
                || context.Content.Is<ProductAttributesPart>())
            {
                if (context.Permission == Orchard.Core.Contents.Permissions.CreateContent) {
                    permission = CommercePermissions.ManageAttributes;
                }
                else if (context.Permission == Orchard.Core.Contents.Permissions.EditContent) {
                    permission = CommercePermissions.ManageAttributes;
                }
                else if (context.Permission == Orchard.Core.Contents.Permissions.PublishContent) {
                    permission = CommercePermissions.ManageAttributes;
                }
                else if (context.Permission == Orchard.Core.Contents.Permissions.DeleteContent) {
                    permission = CommercePermissions.ManageAttributes;
                }

                if (permission != context.Permission) {
                    context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                    context.Permission = permission;
                    context.Adjusted = true;
                }
            }
        }

        public void Complete(CheckAccessContext context) {
        }

        public void Adjust(CheckAccessContext context) {

        }
    }
}
