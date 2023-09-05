using Microsoft.Extensions.Logging;
using MimeKit;
using Piipan.Notifications.Core.Extensions;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    /// <summary>
    /// Construct MimeMessage to send out emails using Mailkit.  This would also attach the image resource as part of the notification templates. 
    /// </summary>
    public class MessageBuilder : IMessageBuilder
    {
        public MessageBuilder()
        {
        }
        /// <summary>
        /// Sends an email based on an inputted email request
        /// </summary>
        /// <param name="emailModel">The email information to be sent including recipients, subject, body, etc..</param>
        /// <param name="footerImagePath">Path of the image resource</param>
        /// <returns>MimeMessage object.</returns>
        public MimeMessage ConstructMimeMessage(EmailModel emailModel, string footerImagePath, EmailDelivery emailDelivery, ILogger logger, Action<Exception> errorCallback)
        {
            var mimeMessage = new MimeMessage();
            try
            {
                mimeMessage.Subject = emailModel.Subject;
                var builder = new BodyBuilder();
                var image = builder.LinkedResources.Add(footerImagePath);
                image.ContentId = "usda-img";
                builder.HtmlBody += emailModel.Body;
                mimeMessage.Body = builder.ToMessageBody();
                mimeMessage.From.Add(MailboxAddress.Parse(emailDelivery.EmailFrom));
                emailModel.ToList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList().ForEach(address => mimeMessage.To.Add(MailboxAddress.Parse(address)));

                if (emailModel.ToCCList != null)
                {
                    emailModel.ToCCList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList().ForEach(address => mimeMessage.Cc.Add(MailboxAddress.Parse(address)));
                }

                if (!string.IsNullOrWhiteSpace(emailDelivery.EmailCc))
                {
                    foreach (string toAddresses in emailDelivery.EmailCc.Trim().Split(',')) // Split on ,
                    {
                        mimeMessage.Cc.Add(MailboxAddress.Parse(toAddresses));
                    }
                }

                if (!string.IsNullOrWhiteSpace(emailDelivery.EmailBcc))
                {
                    foreach (string toAddresses in emailDelivery.EmailBcc.Trim().Split(',')) // Split on ,
                    {
                        mimeMessage.Bcc.Add(MailboxAddress.Parse(toAddresses));
                    }
                }
            }
            catch (Exception ex)
            {
                errorCallback?.Invoke(ex);
                {
                    logger.LogError("Error in constructing notification {0}", ex.Message);
                    logger.LogError(ex, ex.Message);
                    throw;
                }
            }
            return mimeMessage;
        }
    }
}
