using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;
using Piipan.Notifications.Core.Extensions;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Piipan.Shared.Database;
using System.Net.Sockets;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    [Collection("MailServiceSequentialTests")]
    public class MailServiceTest
    {
        IMessageBuilder _messageBuilder;

        public MailServiceTest()
        {
            _messageBuilder = new MessageBuilder();
        }
        private void ClearEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("SmtpCcEmail", null);
            Environment.SetEnvironmentVariable("SmtpBccEmail", null);
            Environment.SetEnvironmentVariable("SmtpFromEmail", null);
            Environment.SetEnvironmentVariable("SmtpServer", null);
        }
        [Fact]
        public async Task FailedSendEmailTest()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");
            var services = new ServiceCollection();
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "Test";
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);
            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

            var smtpClient = new SmtpClient();

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
            var emailService = new MailService(smtpClient, retriever, emailDelivery, _messageBuilder);
            bool errorEncountered = false;

            // Act
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
                {
                    errorEncountered = true;
                });
            });

            // Assert
            Assert.True(errorEncountered);
            logger.Verify(n => n.Log(
                  It.Is<LogLevel>(l => l == LogLevel.Error),
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Failed to send notification.")),
                  It.IsAny<Exception>(),
                  (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once());
            ClearEnvironmentVariables();
        }

        [Fact]
        public async Task SuccessSendEmailTest()
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

            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

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
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, _messageBuilder);
            bool errorEncountered = false;

            // Act
            var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
            {
                errorEncountered = true;
            });

            // Assert
            Assert.True(result);
            Assert.False(errorEncountered);
            logger.Verify(n => n.Log(
                 It.Is<LogLevel>(l => l == LogLevel.Information),
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Email sent successful. Email Subject:")),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
               Times.Once());
            ClearEnvironmentVariables();
        }
        [Fact]
        public async Task SMTPServerFailed()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");

            var services = new ServiceCollection();
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "Test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);


            var smtpClient = new Mock<ISmtpClient>();

            var retriever = Mock.Of<IUsdaImageRetriever>();
            var logger = new Mock<ILogger<IMailService>>();
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
            var emailDelivery = new EmailDelivery()
            {
                Enabled = true,
                EmailCc = Environment.GetEnvironmentVariable("SmtpCcEmail"),
                EmailBcc = Environment.GetEnvironmentVariable("SmtpBccEmail"),
                EmailFrom = Environment.GetEnvironmentVariable("SmtpFromEmail")
            };
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, _messageBuilder);
            bool errorEncountered = false;

            // Act
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
                {
                    errorEncountered = true;
                });
            });

            // Assert
            Assert.True(errorEncountered);
            logger.Verify(n => n.Log(
                   It.Is<LogLevel>(l => l == LogLevel.Error),
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Failed to send notification.")),
                   It.IsAny<Exception>(),
                   (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                 Times.Once());
             ClearEnvironmentVariables();
        }
        [Fact]
        public async Task SMTP_Email_Log_When_Not_Enabled()
        {
            // Arrange
            var services = new ServiceCollection();
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "Test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);


            var smtpClient = new Mock<ISmtpClient>();


            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

            var retriever = retrieverSetup.Object;

            var logger = new Mock<ILogger<IMailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example.com"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };
            var emailDelivery = new EmailDelivery()
            {
                Enabled = false,
                EmailCc = Environment.GetEnvironmentVariable("SmtpCcEmail"),
                EmailBcc = Environment.GetEnvironmentVariable("SmtpBccEmail"),
                EmailFrom = Environment.GetEnvironmentVariable("SmtpFromEmail")
            };
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, _messageBuilder);
            bool errorEncountered = false;

            // Act
            var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
            {

            });

            // Assert
            Assert.True(result);
            // Assert
            logger.Verify(n => n.Log(
                   It.Is<LogLevel>(l => l == LogLevel.Information),
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Enable email is not set. Logged")),
                   It.IsAny<Exception>(),
                   (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                 Times.Once());

            logger.Verify(n => n.Log(
                 It.Is<LogLevel>(l => l == LogLevel.Information),
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Test email body.")),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
               Times.Once());
            ClearEnvironmentVariables();
        }
        [Fact]
        public async Task SuccessSendEmail_When_CC_Email_Address_IsEmpty_Test()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SmtpCcEmail", " ");
            Environment.SetEnvironmentVariable("SmtpBccEmail", " ");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");
            var services = new ServiceCollection();
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);

            var smtpClient = new Mock<ISmtpClient>();

            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

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
                     " "
                },
                ToBCCList = new List<string>
                {
                     " "
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
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, _messageBuilder);
            bool errorEncountered = false;

            // Act
            var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
            {
                errorEncountered = true;
            });

            // Assert
            Assert.True(result);
            Assert.False(errorEncountered);
            logger.Verify(n => n.Log(
                 It.Is<LogLevel>(l => l == LogLevel.Information),
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Email sent successful. Email Subject:")),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
               Times.Once());
            ClearEnvironmentVariables();
        }

        [Fact]
        public async Task BuildMessageBody_Test()
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
           

            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

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

            var messageBuilder = new Mock<IMessageBuilder>();
            var mimeMessage = new MimeMessage();
            messageBuilder.Setup(x => x.ConstructMimeMessage(It.IsAny<EmailModel>(), It.IsAny<string>(), It.IsAny<EmailDelivery>(),logger.Object, It.IsAny<Action<Exception>>())).Returns(mimeMessage);

            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, messageBuilder.Object);
            bool errorEncountered = false;

            // Act
            var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
            {
                errorEncountered = true;
            });

            // Assert   
            Assert.True(result);
            Assert.False(errorEncountered);
            smtpClient.Verify(m => m.Send(mimeMessage, default, null), Times.Once);
            ClearEnvironmentVariables();

        }
        [Fact]
        public async Task SMTP_Email_Log_When_Not_Enabled_Send_Not_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");
            string SmtpConnection = "SmtpServer";
            string SmtpServer = "Test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);


            var smtpClient = new Mock<ISmtpClient>();


            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

            var retriever = retrieverSetup.Object;

            var logger = new Mock<ILogger<IMailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example.com"
                },
                From = "from.test@email.example",
                Subject = "Test Email From Email Service Tests"

            };
            var emailDelivery = new EmailDelivery()
            {
                Enabled = false,
                EmailCc = Environment.GetEnvironmentVariable("SmtpCcEmail"),
                EmailBcc = Environment.GetEnvironmentVariable("SmtpBccEmail"),
                EmailFrom = Environment.GetEnvironmentVariable("SmtpFromEmail")
            };
            var messageBuilder = new Mock<IMessageBuilder>();
            var mimeMessage = new MimeMessage();
            messageBuilder.Setup(x => x.ConstructMimeMessage(It.IsAny<EmailModel>(), It.IsAny<string>(), It.IsAny<EmailDelivery>(), logger.Object, It.IsAny<Action<Exception>>())).Returns(mimeMessage);

            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, messageBuilder.Object);
            bool errorEncountered = false;

            // Act
            var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
            {

            });

            // Assert
            Assert.True(result);
            // Assert
            smtpClient.Verify(m => m.Send(mimeMessage, default, null), Times.Never);
            ClearEnvironmentVariables();
        }
        [Fact]
        public async Task SMTPServerFailed_SMTP_Error()
        {
            var services = new ServiceCollection();
            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");
            string SmtpConnection = "SmtpServer"; 
            string SmtpServer = "test"; // 
            Environment.SetEnvironmentVariable(SmtpConnection, SmtpServer);


            var smtpClient = new Mock<ISmtpClient>();

            smtpClient.Setup(x => x.Connect(Environment.GetEnvironmentVariable("SmtpServer"), 587, SecureSocketOptions.None, default)).Throws(new Exception("No connection could be made"));



            var retrieverSetup = new Mock<IUsdaImageRetriever>();
            retrieverSetup.Setup(x => x.RetrieveUsdaSymbolColorImagePath()).Returns("images/usda-symbol-color.png");

            var retriever = retrieverSetup.Object;

            var logger = new Mock<ILogger<IMailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "test@email.example.com"
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
            var emailService = new MailService(smtpClient.Object, retriever, emailDelivery, _messageBuilder);
            bool errorEncountered = false;

            // Act
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                var result = await emailService.SendEmailAsync(emailModel, logger.Object, (ex) =>
                {
                    errorEncountered = true;
                });
            });
            // Assert
            Assert.True(errorEncountered);
            logger.Verify(n => n.Log(
                  It.Is<LogLevel>(l => l == LogLevel.Error),
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"No connection could be made")),
                  It.IsAny<Exception>(),
                  (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce());
            ClearEnvironmentVariables();
        }
    }
}
