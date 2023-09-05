using Microsoft.Extensions.Logging;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(EmailModel emailModel, ILogger logger, Action<Exception> errorCallback);

    }
}
