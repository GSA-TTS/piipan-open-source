using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Piipan.Notifications.Core.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Func.Api.Tests
{
    public class NotificationRequestProcessorTest
    {
        private void VerifyLogError(Mock<ILogger> logger, String expected)
        {
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expected),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
        [Fact]
        public async Task Run_Success()
        {
            // Arrange
            var emailModel = new EmailModel
            {

                ToList = "Test@Test.com".Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };

            EmailModelRequest emailRequest = new EmailModelRequest() { Data = emailModel };

            string input = JsonConvert.SerializeObject(emailRequest);
            var logger = new Mock<ILogger>();
            var mailService = new Mock<IMailService>();

            mailService
           .Setup(m => m.SendEmailAsync(emailModel, logger.Object, It.IsAny<Action<Exception>>()));

            var function = new NotificationApi(mailService.Object);
            // Act 
            await function.Run(input, logger.Object);
            // Assert

            mailService.Verify(m => m.SendEmailAsync(It.IsAny<EmailModel>(), logger.Object, It.IsAny<Action<Exception>>()), Times.Once);
        }
        [Fact]
        public async Task Run_ApiThrows()
        {
            // Arrange
            var emailModel = new EmailModel
            {

                ToList = "Test@Test.com".Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };

            EmailModelRequest emailRequest = new EmailModelRequest() { Data = emailModel };

            string input = JsonConvert.SerializeObject(emailRequest);
            var logger = new Mock<ILogger>();
            var mailService = new Mock<IMailService>();

            mailService
                .Setup(m => m.SendEmailAsync(It.IsAny<EmailModel>(), logger.Object, It.IsAny<Action<Exception>>()))
                .Callback<EmailModel, ILogger, Action<Exception>>((emailModel, logger, errorCallback) =>
                {
                    Exception exception = new Exception("the api broke");
                    errorCallback.Invoke(exception);
                    throw exception;
                });

            var function = new NotificationApi(mailService.Object);
            // Act 
            await Assert.ThrowsAsync<Exception>(() => function.Run(input, logger.Object));
            // Assert
            VerifyLogError(logger, "the api broke");
        }
        [Fact]
        public async void Run_NullInputStream()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var mailService = new Mock<IMailService>();
            var function = new NotificationApi(mailService.Object);
            // Act 
            await function.Run("", logger.Object);

            // Assert
            VerifyLogError(logger, "No input was provided");
        }

        [Fact]
        public async void Run_EmptyQueue()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var mailService = new Mock<IMailService>();
            var function = new NotificationApi(mailService.Object);
            // Act 
            await function.Run("", logger.Object);

            // Assert
            VerifyLogError(logger, "No input was provided");
        }
    }
}
