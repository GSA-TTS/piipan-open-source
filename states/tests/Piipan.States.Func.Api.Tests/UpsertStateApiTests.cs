using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Http;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Models;
using Piipan.States.Core.Parsers;
using Xunit;

namespace Piipan.States.Func.Api.Tests
{
    public class UpsertStateApiTests
    {

        static UpsertState Construct(Mock<IStateInfoDao> mock)
        {
            var stateInfoDao = mock;
            var requestParser = new StateInfoRequestParser(
                Mock.Of<ILogger<StateInfoRequestParser>>());

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";

            var cryptographyClient = new AzureAesCryptographyClient(key);

            var upsertState = new UpsertState(
                stateInfoDao.Object, requestParser, cryptographyClient
            );
            return upsertState;
        }

        private static Mock<HttpRequest> MockRequest(string jsonBody = "{}")
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
                { "X-Initiating-State", "ea"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async Task UpsertState_LogsRequest()
        {
            // Arrange
            var api = Construct(new Mock<IStateInfoDao>());
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
        public async Task UpsertState_Returns500IfExceptionOccurs()
        {
            // Arrange
            var mockRequest = MockRequest("{ \"data\": { \"email\": \"fakeEmail\" } }");
            
            var logger = new Mock<ILogger>();
            // Mocks
            var statesDao = new Mock<IStateInfoDao>();
            statesDao
                .Setup(r => r.UpsertState(It.IsAny<StateInfoDto>()))
                .ThrowsAsync(new Exception("Some database error"));

            var requestParser = new StateInfoRequestParser(
                Mock.Of<ILogger<StateInfoRequestParser>>());
            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var api = new UpsertState(
                statesDao.Object, requestParser, cryptographyClient);

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as JsonResult;
            Assert.Equal(500, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("500", errorResponse.Errors[0].Status);
            Assert.Equal("Some database error", errorResponse.Errors[0].Detail);
            Assert.Contains("Exception", errorResponse.Errors[0].Title);
        }

        [Fact]
        public async Task UpsertState_Returns200()
        {
            // Arrange
            var mockRequest = MockRequest("{ \"data\": { \"email\": \"fakeEmail...\" } }");

            var logger = new Mock<ILogger>();
            // Mocks
            var statesDao = new Mock<IStateInfoDao>();
            statesDao
                .Setup(r => r.UpsertState(It.IsAny<StateInfoDto>()))
                .ReturnsAsync(1);

            var requestParser = new StateInfoRequestParser(
                Mock.Of<ILogger<StateInfoRequestParser>>());
            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var api = new UpsertState(
                statesDao.Object, requestParser, cryptographyClient);

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as JsonResult;
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task UpsertState_Returns500()
        {
            // Arrange
            var mockRequest = MockRequest("{ \"data\": { \"email\": \"fakeEmail...\" } }");

            var logger = new Mock<ILogger>();
            // Mocks
            var statesDao = new Mock<IStateInfoDao>();
            statesDao
                .Setup(r => r.UpsertState(It.IsAny<StateInfoDto>()))
                .ReturnsAsync(0);

            var requestParser = new StateInfoRequestParser(
                Mock.Of<ILogger<StateInfoRequestParser>>());
            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var api = new UpsertState(
                statesDao.Object, requestParser, cryptographyClient);

            // Act
            var response = await api.Run(mockRequest.Object, logger.Object);

            // Assert
            var result = response as JsonResult;
            Assert.Equal(500, result.StatusCode);
        }
    }
}
