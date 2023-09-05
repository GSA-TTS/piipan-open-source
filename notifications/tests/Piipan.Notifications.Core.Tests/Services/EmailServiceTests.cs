using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Xunit;
namespace Piipan.Notifications.Core.Tests.Services
{
    public class EmailServiceTests
    {
        /// <summary>
        /// This code will try to send and email provided you replace the from with a valid google workspace email that also has "Less secure app access" set to ON
        /// </summary>
        [Fact]
        public async void SuccessfulSendEmailSingleRecipient()
        {
            // Arrange
            var logger = Mock.Of<ILogger<EmailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };

            var emailService = new EmailService("smtp.gmail.com", "from.test@email.example", "Deadbeef123!", logger);

            // Act
            var result = await emailService.SendEmail(emailModel);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// This code will try to send and email provided you replace the from with a valid google workspace email that also has "Less secure app access" set to ON
        /// </summary>
        [Fact]
        public async void SuccessfulSendEmailMultipleRecipienst()
        {
            // Arrange
            var logger = Mock.Of<ILogger<EmailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example",
                     "test2@email.example"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };

            var emailService = new EmailService("smtp.gmail.com", "from.test@email.example", "Deadbeef123!", logger);

            // Act
            var result = await emailService.SendEmail(emailModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async void FailedSendEmail()
        {
            // Arrange
            var logger = Mock.Of<ILogger<EmailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };

            var emailService = new EmailService("smtp.email.com", "bad-account@email.invalid", "FalsePassword", logger);

            // Act
            var result = await emailService.SendEmail(emailModel);

            // Assert
            Assert.True(result); // Need to work on Test case when the mail client is decided.
        }
    }
}
