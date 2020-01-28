using Nwazet.Commerce.Models;
using Orchard.ContentManagement;

namespace Nwazet.Commerce.Services {
    public class UpdateStatusService : IUpdateStatusService {
        private readonly IContentManager _contentManager;

        public UpdateStatusService(
            IContentManager contentManager
            ) {
            _contentManager = contentManager;
        }

        public void UpdateOrderStatusChanged(OrderPart part, string newStatus) {
            if (part.Status != newStatus) {
                part.Status = newStatus;
                _contentManager.Publish(part.ContentItem);
            }
        }
    }
}
