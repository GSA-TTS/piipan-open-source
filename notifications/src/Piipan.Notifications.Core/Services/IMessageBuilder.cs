using Microsoft.Extensions.Logging;
using MimeKit;
using Piipan.Notifications.Core.Extensions;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public interface IMessageBuilder
    {
        MimeMessage ConstructMimeMessage(EmailModel emailModel, string footerImagePath, EmailDelivery emailDelivery, ILogger logger, Action<Exception> errorCallback);
    }
}