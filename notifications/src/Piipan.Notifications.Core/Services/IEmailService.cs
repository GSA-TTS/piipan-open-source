using Piipan.Notifications.Models;

namespace Piipan.Notifications.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(EmailModel emailDetails);
    }
}
