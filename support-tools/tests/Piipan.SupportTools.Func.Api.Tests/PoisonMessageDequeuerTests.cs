using System.Web.Http;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.SupportTools.Core.Models;
using Piipan.SupportTools.Core.Service;
using Xunit;

namespace Piipan.SupportTools.Func.Api.Tests
{
    public class PoisonMessageDequeuerTests
    {
        private Mock<HttpRequest> MockRequest()
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write("{ \"queue_name\": \"fakeQueueName\", \"account_name\": \"fakeAccountName\", \"account_key\": \"fakeAccountKey\" }");
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            return mockRequest;
        }

        [Fact]
        public async Task PoisonMessageDequeuer_LogsRequest()
        {
            // Arrange
            var mockPoisonMessageService = GetMockPoisonMessageService();

            var api = new PoisonMessageDequeuer(mockPoisonMessageService.Object);
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            await api.Run(mockRequest.Object, logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing request from user")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task PoisonMessageDequeuer_ReturnsBadRequestResponse()
        {
            // Arrange
            var mockPoisonMessageService = new Mock<IPoisonMessageService>();
            mockPoisonMessageService.Setup(c => c.Parse(It.IsAny<Stream>())).Returns(Task.FromResult(new PoisonMessageDequeuerParam()));

            var api = new PoisonMessageDequeuer(mockPoisonMessageService.Object);
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal("The following information is necessary: Queue Name, Account Name, and Account Key", result?.Value);
        }

        [Fact]
        public async Task PoisonMessageDequeuer_ReturnsBadRequestResponse_QueueIsNotExist()
        {
            // Arrange
            var mockPoisonMessageService = GetMockPoisonMessageService();

            mockPoisonMessageService.Setup(c => c.RetryPoisonMessages(It.IsAny<QueueClient>(), It.IsAny<QueueClient>()))
                .Throws(new HttpRequestException("Target and/or Poison Queues do not exist. The Poison Queue must be named '{target queue name}-poison'"));

            var api = new PoisonMessageDequeuer(mockPoisonMessageService.Object);
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal("Target and/or Poison Queues do not exist. The Poison Queue must be named '{target queue name}-poison'", result?.Value);
        }

        [Fact]
        public async Task PoisonMessageDequeuer_ReturnsOkResponse()
        {
            // Arrange
            var mockPoisonMessageService = GetMockPoisonMessageService();

            mockPoisonMessageService.Setup(c => c.RetryPoisonMessages(It.IsAny<QueueClient>(), It.IsAny<QueueClient>())).Returns(Task.FromResult(10));

            var api = new PoisonMessageDequeuer(mockPoisonMessageService.Object);
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);
            Assert.Contains("Received 10", result?.Value?.ToString());
        }

        [Fact]
        public async Task PoisonMessageDequeuer_Returns500IfExceptionOccurs()
        {
            // Arrange
            var mockPoisonMessageService = new Mock<IPoisonMessageService>();

            var api = new PoisonMessageDequeuer(mockPoisonMessageService.Object);
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as InternalServerErrorResult;
            Assert.Equal(500, result.StatusCode);
        }

        private Mock<IPoisonMessageService> GetMockPoisonMessageService()
        {
            var mockPoisonMessageService = new Mock<IPoisonMessageService>();
            mockPoisonMessageService.Setup(c => c.Parse(It.IsAny<Stream>())).Returns(Task.FromResult(new PoisonMessageDequeuerParam()
            {
                AccountKey = "122Bz6pg3qEsqvTgY12XVFrJ+DG+7hsTlki8yF1TESTs+goKUM+ewl5VZEd7VX9pLbC2vLE7tu2X+AStpvhoJQ==",
                AccountName = "fakeAccountName",
                QueueName = "fakeQueueName"
            }));

            return mockPoisonMessageService;
        }
    }
}