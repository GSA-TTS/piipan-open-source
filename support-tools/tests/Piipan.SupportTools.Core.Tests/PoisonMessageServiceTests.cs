using System.Reflection;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Piipan.Shared.Tests.Helpers;
using Piipan.SupportTools.Core.Service;
using Xunit;

namespace Piipan.SupportTools.Core.Tests
{
    public class PoisonMessageServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(200)]
        [InlineData(1000)]
        [InlineData(258654)]
        public async void RetryPoisonMesssages_RetryPoisonMesssages(int countOfMessages)
        {
            //Arrange
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();

            var mockPoisonQueueClient = GetMockPoisonQueue(countOfMessages, false);
            var mockTargetQueueClient = GetMockTargetQueue(true);

            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            var res = await service.RetryPoisonMessages(mockTargetQueueClient.Object, mockPoisonQueueClient.Object);

            //Assert
            Assert.True(res < PoisonMessageService.MaxLimitMessagesPerExecution ? countOfMessages == res
                : PoisonMessageService.MaxLimitMessagesPerExecution == res);

            mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => countOfMessages == 0 ? v.ToString().Contains("There are no messages in poison queue")
                : v.ToString().Contains($"Received {(countOfMessages > PoisonMessageService.MaxLimitMessagesPerExecution ? PoisonMessageService.MaxLimitMessagesPerExecution : countOfMessages)} " +
                "messages from poison queue and sent them to regular queue")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            AssertRetryPoisonMesssages(mockPoisonQueueClient, mockTargetQueueClient, countOfMessages,
                Times.Exactly(countOfMessages > PoisonMessageService.MaxLimitMessagesPerExecution
                ? PoisonMessageService.MaxLimitMessagesPerExecution : countOfMessages));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(200)]
        [InlineData(1000)]
        [InlineData(258654)]
        public async void RetryPoisonMesssages_RetryPoisonMesssages_SendMethodFailed(int countOfMessages)
        {
            //Arrange
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();

            var mockPoisonQueueClient = GetMockPoisonQueue(countOfMessages, false);
            var mockTargetQueueClient = GetMockTargetQueue(false);

            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            var res = await service.RetryPoisonMessages(mockTargetQueueClient.Object, mockPoisonQueueClient.Object);

            //Assert
            Assert.True(res == 0);

            mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There are no messages in poison queue")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            AssertRetryPoisonMesssages(mockPoisonQueueClient, mockTargetQueueClient, countOfMessages, Times.Never());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(200)]
        [InlineData(1000)]
        [InlineData(258654)]
        public async void RetryPoisonMesssages_RetryPoisonMesssages_DeleteMethodFailed(int countOfMessages)
        {
            //Arrange
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();

            var mockPoisonQueueClient = GetMockPoisonQueue(countOfMessages, true);
            var mockTargetQueueClient = GetMockTargetQueue(true);

            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            var res = await service.RetryPoisonMessages(mockTargetQueueClient.Object, mockPoisonQueueClient.Object);

            //Assert
            Assert.True(res < PoisonMessageService.MaxLimitMessagesPerExecution ? countOfMessages == res : PoisonMessageService.MaxLimitMessagesPerExecution == res);

            mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("RetryPoisonMesssages: DeleteMessageAsync:: error status")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            AssertRetryPoisonMesssages(mockPoisonQueueClient, mockTargetQueueClient, countOfMessages,
                Times.Exactly(countOfMessages > PoisonMessageService.MaxLimitMessagesPerExecution ? PoisonMessageService.MaxLimitMessagesPerExecution : countOfMessages));
        }


        [Fact]
        public async void RetryPoisonMesssages_ClientIsNotExist()
        {
            //Arrange
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();

            var mockPoisonQueueClient = new Mock<QueueClient>();
            var mockTargetQueueClient = new Mock<QueueClient>();

            var mockExistResponse = new Mock<Response<bool>>();
            mockExistResponse.SetupGet(m => m.Value).Returns(false);

            mockPoisonQueueClient.Setup(c => c.Exists(It.IsAny<CancellationToken>())).Returns(mockExistResponse.Object);
            mockTargetQueueClient.Setup(c => c.Exists(It.IsAny<CancellationToken>())).Returns(mockExistResponse.Object);

            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            try
            {
                var res = await service.RetryPoisonMessages(mockTargetQueueClient.Object, mockPoisonQueueClient.Object);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.True(ex.Message == "Target and/or Poison Queues do not exist. The Poison Queue must be named '{target queue name}-poison'");
                mockTargetQueueClient.Verify(c => c.Exists(It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact]
        public async void Parse_ShouldReturnObject()
        {
            //Arrange
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            var queue_name = "fakeQueueName";
            var account_name = "fakeAccountName";
            var account_key = "fakeAccountKey";

            sw.Write("{ \"queue_name\": \"" + queue_name + "\", \"account_name\": \"" + account_name + "\", \"account_key\": \"" + account_key + "\" }");
            sw.Flush();

            ms.Position = 0;
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();
            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            var result = await service.Parse(ms);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.AccountKey == account_key);
            Assert.True(result.QueueName == queue_name);
            Assert.True(result.AccountName == account_name);
        }

        [Fact]
        public async void Parse_ShouldReturnException()
        {
            //Arrange
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);


            sw.Write("");
            sw.Flush();

            ms.Position = 0;
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();
            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            try
            {
                var result = await service.Parse(ms);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.True(ex is JsonSerializationException);
                Assert.True(ex.Message == "Stream must not be empty.");
            }
        }

        [Fact]
        public async void Parse_ShouldReturnException_StreamIsNull()
        {
            //Arrange
            var mockLogger = new Mock<ILogger<PoisonMessageService>>();
            var service = new PoisonMessageService(mockLogger.Object);

            //Act
            try
            {
                var result = await service.Parse(null);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.True(ex is JsonSerializationException);
                Assert.True(ex.Message == "Request Body must not be null.");
            }
        }

        private Mock<Response<QueueProperties>> GetMockPropertiesResponse(int countOfMessages)
        {
            var mockPropertiesResponse = new Mock<Response<QueueProperties>>();
            var properties = new QueueProperties();
            // little hack since ApproximateMessagesCount has internal setter
            properties.GetType().GetProperty(nameof(properties.ApproximateMessagesCount), BindingFlags.Public | BindingFlags.Instance)?.SetValue(properties, countOfMessages);
            mockPropertiesResponse.SetupGet(r => r.Value).Returns(properties);

            return mockPropertiesResponse;
        }

        private Mock<QueueClient> GetMockPoisonQueue(int countOfMessages, bool isDeleteMessageFailed)
        {
            var mockExistResponse = new Mock<Response<bool>>();
            mockExistResponse.SetupGet(m => m.Value).Returns(true);

            var mockMessageSecondReponse = new Mock<Response<QueueMessage[]>>();
            mockMessageSecondReponse.SetupGet(m => m.Value);

            var mockPoisonQueueClient = new Mock<QueueClient>();
            mockPoisonQueueClient.Setup(c => c.Exists(It.IsAny<CancellationToken>())).Returns(mockExistResponse.Object);
            var setupSequenceResult = mockPoisonQueueClient.SetupSequence(c => c.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()));

            var listQueue = new QueueMessage[countOfMessages];
            foreach (var messages in listQueue.Batch(PoisonMessageService.MaxMessasges))
            {
                var mockMessageReponse = new Mock<Response<QueueMessage[]>>();
                mockMessageReponse.SetupGet(m => m.Value).Returns(messages.ToArray());

                setupSequenceResult.Returns(Task.FromResult(mockMessageReponse.Object));
            }

            setupSequenceResult.Returns(Task.FromResult(mockMessageSecondReponse.Object));

            mockPoisonQueueClient.Setup(c => c.GetProperties(It.IsAny<CancellationToken>())).Returns(GetMockPropertiesResponse(countOfMessages).Object);

            var mockDeleteResponse = new Mock<Response>();
            mockDeleteResponse.SetupGet(m => m.IsError).Returns(isDeleteMessageFailed);

            mockPoisonQueueClient.Setup(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockDeleteResponse.Object));

            return mockPoisonQueueClient;
        }

        private void AssertRetryPoisonMesssages(Mock<QueueClient> mockPoisonQueueClient, Mock<QueueClient> mockTargetQueueClient, int countOfMessages, Times times)
        {
            mockPoisonQueueClient.Verify(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), times);

            mockTargetQueueClient.Verify(c => c.SendMessageAsync(It.IsAny<BinaryData>(), null, null,
                It.IsAny<CancellationToken>()), Times.Exactly(countOfMessages > PoisonMessageService.MaxLimitMessagesPerExecution
                ? PoisonMessageService.MaxLimitMessagesPerExecution : countOfMessages));

            mockPoisonQueueClient.Verify(c => c.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()), Times.Exactly(countOfMessages == 0 ? 0
                : countOfMessages <= PoisonMessageService.MaxMessasges ? 1
                : countOfMessages > PoisonMessageService.MaxLimitMessagesPerExecution ? PoisonMessageService.MaxLimitMessagesPerExecution / PoisonMessageService.MaxMessasges
                : countOfMessages % PoisonMessageService.MaxMessasges == 0 ? countOfMessages / PoisonMessageService.MaxMessasges
                : countOfMessages / PoisonMessageService.MaxMessasges + 1));

            mockPoisonQueueClient.Verify(c => c.Exists(It.IsAny<CancellationToken>()), Times.Once);
            mockTargetQueueClient.Verify(c => c.Exists(It.IsAny<CancellationToken>()), Times.Once);
        }

        private Mock<QueueClient> GetMockTargetQueue(bool shouldSendMessageReturnValue)
        {
            var mockExistResponse = new Mock<Response<bool>>();
            mockExistResponse.SetupGet(m => m.Value).Returns(true);

            var mockTargetQueueClient = new Mock<QueueClient>();

            if (shouldSendMessageReturnValue)
            {
                var mockSendReceipt = new Mock<SendReceipt>();
                var mockSendResponse = new Mock<Response<SendReceipt>>();
                mockSendResponse.SetupGet(m => m.Value).Returns(mockSendReceipt.Object);

                mockTargetQueueClient.Setup(c => c.SendMessageAsync(It.IsAny<BinaryData>(), null, null, It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(mockSendResponse.Object));
            }
            else
            {
                mockTargetQueueClient.Setup(c => c.SendMessageAsync(It.IsAny<BinaryData>(), null, null, It.IsAny<CancellationToken>()));
            }

            mockTargetQueueClient.Setup(c => c.Exists(It.IsAny<CancellationToken>())).Returns(mockExistResponse.Object);

            return mockTargetQueueClient;
        }
    }
}