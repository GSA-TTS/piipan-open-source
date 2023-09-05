using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Services
{
    public class EmailService : IEmailService
    {
        private SmtpClient smtpClient;
        private readonly ILogger<EmailService> _logger;
        public EmailService(string smtpServerName, string credentialsUser, string credentialsPassword, ILogger<EmailService> logger)
        {
            _logger = logger;
            smtpClient = new SmtpClient(smtpServerName)
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(credentialsUser, credentialsPassword),
                EnableSsl = true
            };
        }
        public async Task<bool> SendEmail(EmailModel emailDetails)
        {
            //try
            //{
            //    string toList = String.Join(",", emailDetails.ToList);
            //    smtpClient.Send(emailDetails.From, toList, emailDetails.Subject, emailDetails.Body);
            //}
            //catch (Exception exception)
            //{
            //    _logger.LogError(exception.ToString());
            //    return await Task.FromResult(false);
            //}
            return await Task.FromResult(true);
        }
    }
}