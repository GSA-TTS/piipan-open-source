using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;
using Piipan.Notifications.Core.Extensions;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    [Collection("MailServiceSequentialTests")]
    public class MessageBuilderTest
    {
        private void ClearEnvironmentVariables()
        {

            Environment.SetEnvironmentVariable("SmtpCcEmail", null);
            Environment.SetEnvironmentVariable("SmtpBccEmail", null);
            Environment.SetEnvironmentVariable("SmtpFromEmail", null);
            Environment.SetEnvironmentVariable("SmtpServer", null);
        }

        [Fact]
        public async void MessageBuilder_Test()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");
            var services = new ServiceCollection();
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);

            var smtpClient = new Mock<ISmtpClient>();

            var retrieverSetup = new Mock<IUsImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsSymbolColorImagePath()).Returns("images/18f-symbol-color.png");

            var retriever = retrieverSetup.Object;

            

            var logger = new Mock<ILogger<IMailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example"
                },
                ToCCList = new List<string>
                {
                     "testCC@email.example"
                },
                ToBCCList = new List<string>
                {
                     "testBcc@email.example"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };
            var emailDelivery = new EmailDelivery()
            {
                Enabled = true,
                EmailCc = Environment.GetEnvironmentVariable("SmtpCcEmail"),
                EmailBcc = Environment.GetEnvironmentVariable("SmtpBccEmail"),
                EmailFrom = Environment.GetEnvironmentVariable("SmtpFromEmail")
            };

            MimeMessage message = new MimeMessage();

            message.Subject = "Test Email From Email Service Tests";
            message.To.Add(MailboxAddress.Parse(@"test@email.example"));

            message.Cc.Add(MailboxAddress.Parse(@"testCC@email.example"));
            message.Cc.Add(MailboxAddress.Parse(@"test-cc@example.com"));

            message.Bcc.Add(MailboxAddress.Parse(@"test-bcc@example.com"));
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = "Test email body.";
            bodyBuilder.TextBody = "Test email body.";
            message.Body = bodyBuilder.ToMessageBody();
            message.Body.ContentId  = @"18f-img";
            message.Body.ContentType.Name = @"18f-symbol-color.png";

            var messageBuilder = new MessageBuilder();
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, messageBuilder);

            var mimeMessage = messageBuilder.ConstructMimeMessage(emailModel, "images/18f-symbol-color.png", emailDelivery, logger.Object, It.IsAny<Action<Exception>>());
            // Assert   

            Assert.Equal(message.To.ToString(), mimeMessage.To.ToString());
            Assert.Equal(message.Cc.ToString(), mimeMessage.Cc.ToString());
            Assert.Equal(message.Bcc.ToString(), mimeMessage.Bcc.ToString());

            Assert.Equal(message.Subject, mimeMessage.Subject);
            Assert.True(mimeMessage.HtmlBody.Contains("Test email body."));


            Assert.Contains(message.Body.ContentId, mimeMessage.BodyParts.OfType<MimePart>().Select(part => part.ContentId).ToList());

            Assert.NotNull(mimeMessage.BodyParts.OfType<MimePart>().FirstOrDefault(part => part?.ContentType.Name?.Contains(message.Body.ContentType.Name)??false));

            ClearEnvironmentVariables();

        }
          [Fact]
        public async void MessageBuilder_Empty_From_Address_Test()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "");
            var services = new ServiceCollection();
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);

            var smtpClient = new Mock<ISmtpClient>();

            var retrieverSetup = new Mock<IUsImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsSymbolColorImagePath()).Returns("images/18f-symbol-color.png");

            var retriever = retrieverSetup.Object;

            

            var logger = new Mock<ILogger<IMailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example"
                },
                ToCCList = new List<string>
                {
                     "testCC@email.example"
                },
                ToBCCList = new List<string>
                {
                     "testBcc@email.example"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };
            var emailDelivery = new EmailDelivery()
            {
                Enabled = true,
                EmailCc = Environment.GetEnvironmentVariable("SmtpCcEmail"),
                EmailBcc = Environment.GetEnvironmentVariable("SmtpBccEmail"),
                EmailFrom = Environment.GetEnvironmentVariable("SmtpFromEmail")
            };

            MimeMessage message = new MimeMessage();

            message.Subject = "Test Email From Email Service Tests";
            message.To.Add(MailboxAddress.Parse(@"test@email.example"));

            message.Cc.Add(MailboxAddress.Parse(@"testCC@email.example"));
            message.Cc.Add(MailboxAddress.Parse(@"test-cc@example.com"));

            message.Bcc.Add(MailboxAddress.Parse(@"test-bcc@example.com"));
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = "Test email body.";
            bodyBuilder.TextBody = "Test email body.";
            message.Body = bodyBuilder.ToMessageBody();
            message.Body.ContentId  = @"18f-img";
            message.Body.ContentType.Name = @"18f-symbol-color.png";

            var messageBuilder = new MessageBuilder();
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, messageBuilder);

            var mimeMessage = new MimeMessage();
            var errorEncountered = false;


             await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                mimeMessage = messageBuilder.ConstructMimeMessage(emailModel, "images/18f-symbol-color.png", emailDelivery, logger.Object, (ex) =>
                {
                    errorEncountered = true;
                });
            });
            // Assert   

            Assert.True(errorEncountered);
            logger.Verify(n => n.Log(
                  It.Is<LogLevel>(l => l == LogLevel.Error),
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Error in constructing notification")),
                  It.IsAny<Exception>(),
                  (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce());
            ClearEnvironmentVariables();


        }
       
    }
}
