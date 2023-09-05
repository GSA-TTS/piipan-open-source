using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Piipan.Notifications.Core.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Func.Api;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.IntegrationTests
{
    public class NotificationApiTest
    {
        private EventGridEvent MockBadEvent(DateTime eventTime, string State)
        {
            var gridEvent = new Mock<EventGridEvent>("", "", "", new BinaryData(new
            {
            }));

            return gridEvent.Object;
        }

        [Fact]
        public async Task Run_Log_Success()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();
            var mailService = new Mock<IMailService>();
            var function = new NotificationApi(mailService.Object);

            var emailModel = new EmailModel
            {
                ToList = "Test@Test.com".Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };
            EmailModelRequest emailRequest = new EmailModelRequest() { Data = emailModel };

            string input = JsonConvert.SerializeObject(emailRequest);

            // Act
            await function.Run(input, logger.Object);

            // Assert
            logger.Verify(x => x.Log(
               It.Is<LogLevel>(l => l == LogLevel.Information),
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(emailModel.Body)),
               It.IsAny<Exception>(),
               It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
           ));
            logger.Verify(x => x.Log(
              It.Is<LogLevel>(l => l == LogLevel.Information),
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(emailModel.Subject)),
              It.IsAny<Exception>(),
              It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
          ));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Email Queue trigger function processed:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
        [Fact]
        public async Task Run_Log_BadRequest()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var mailService = new Mock<IMailService>();
            var function = new NotificationApi(mailService.Object);

            // Act
            await function.Run("", logger.Object);

            // Assert

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "No input was provided"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}