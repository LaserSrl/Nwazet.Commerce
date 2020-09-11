using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Settings;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Nwazet.Commerce.Handlers {
    public class FlexibleShippingMethodPartAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) {
            Permission permission = context.Permission;
            if (context.Content.Is<FlexibleShippingMethodPart>()) {
                var typeDefinition = context.Content.ContentItem.TypeDefinition;
                // adjusting permissions only if the content is not securable
                if (!typeDefinition.Settings.GetModel<ContentTypeSettings>().Securable) {
                    if (context.Permission == Orchard.Core.Contents.Permissions.CreateContent) {
                        permission = CommercePermissions.ManageShipping;
                    }
                    else if (context.Permission == Orchard.Core.Contents.Permissions.EditContent) {
                        permission = CommercePermissions.ManageShipping;
                    }
                    else if (context.Permission == Orchard.Core.Contents.Permissions.PublishContent) {
                        permission = CommercePermissions.ManageShipping;
                    }
                    else if (context.Permission == Orchard.Core.Contents.Permissions.DeleteContent) {
                        permission = CommercePermissions.ManageShipping;
                    }

                    if (permission != context.Permission) {
                        context.Granted = false; //Force granted to false so next adjust iteration will check against the new permission starting from an unauthorized condition
                        context.Permission = permission;
                        context.Adjusted = true;
                    }
                }
            }
        }
        public void Complete(CheckAccessContext context) {
        }

        public void Adjust(CheckAccessContext context) {
        }
    }
}
