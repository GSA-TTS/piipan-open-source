using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public interface INotificationPublish
    {
        Task PublishEmail(EmailModel emailModel);
    }
}
