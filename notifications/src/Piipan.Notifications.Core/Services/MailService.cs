using Microsoft.Extensions.Logging;
using Piipan.Notifications.Core.Extensions;
using Piipan.Notifications.Models;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Piipan.Notifications.Core.Services
{
    /// <summary>
    /// Service layer for sending emails
    /// </summary>
    public class MailService : IMailService
    {
        private readonly ISmtpClient _smtpClient;
        private readonly EmailDelivery _emailDelivery; //Defined in Configuration : "EnableEmails","SmtpCcEmail", "SmtpBccEmail", "SmtpFromEmail" 
        private readonly IUsdaImageRetriever _usdaImageRetriever;
        private readonly IMessageBuilder _messageBuilder;

        public MailService(ISmtpClient smtpClient, IUsdaImageRetriever usdaImageRetriever, EmailDelivery emailDelivery, IMessageBuilder messageBuilder)
        {
            _smtpClient = smtpClient;
            _usdaImageRetriever = usdaImageRetriever;
            _emailDelivery = emailDelivery;
            _messageBuilder = messageBuilder;
        }
        /// <summary>
        /// Sends an email based on an inputted email request
        /// </summary>
        /// <param name="emailModel">The email information to be sent including recipients, subject, body, etc..</param>
        /// <param name="errorCallback">Callback method to invoke if the email fails to send</param>
        /// <returns>Returns True if email was sent to smtp successfully. Returns false if not.</returns>
        public async Task<bool> SendEmailAsync(EmailModel emailModel, ILogger logger, Action<Exception> errorCallback)
        {
            try
            {
                MimeMessage mimeMessage = _messageBuilder.ConstructMimeMessage(emailModel, _usdaImageRetriever.RetrieveUsdaSymbolColorImagePath(), _emailDelivery, logger, (ex) =>
                {
                    logger.LogError(ex.Message);
                }); 

                
                if (_emailDelivery.Enabled)
                {
                    try
                    {
                        _smtpClient.Connect(Environment.GetEnvironmentVariable("SmtpServer"), 587, SecureSocketOptions.None);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Error trying to connect: {0}", ex.Message);
                        logger.LogError(ex, ex.Message);
                        throw;
                    }
                    try
                    {
                        _smtpClient.Send(mimeMessage);
                        logger.LogInformation($"Email sent successful. " + mimeMessage.ToStringExtended(emailModel.Body));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Error sending message: {0}", ex.Message);
                        logger.LogError(ex, ex.Message);
                    }
                    if (_smtpClient.IsConnected)
                        _smtpClient.Disconnect(true);
                }
                else
                    // Emails is not enabled to send out.  Log the message the return true;
                    logger.LogInformation($"Enable email is not set. Logged " + mimeMessage.ToStringExtended(emailModel.Body));

                return true;
            }
            catch (Exception ex)
            {
                errorCallback?.Invoke(ex);
                {
                    logger.LogError("Failed to send notification. {0} - Logged Email:{1}", ex.Message, emailModel.Body);
                    logger.LogError(ex, ex.Message);
                    throw;
                }
            }

        }
    }
}