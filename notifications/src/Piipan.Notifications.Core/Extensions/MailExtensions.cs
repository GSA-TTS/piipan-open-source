using MimeKit;

namespace Piipan.Notifications.Core.Extensions
{
    public static class MailExtensions
    {
        public static string ToStringExtended(this MimeMessage mailMessage, string emailBody)
        {
            return String.Format("Email Subject: {0} - Email Body: {1}",  mailMessage.Subject, emailBody);
        }
    }
}
