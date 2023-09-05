using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Shared.Http;
using Piipan.States.Api.Models;
using Piipan.States.Core.Service;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.States.Func.Api.Tests
{
    public class StateApiTests
    {

        static StateApi Construct()
        {
            var stateInfoDao = new Mock<IStateInfoService>();
            var api = new StateApi(
                stateInfoDao.Object
            );
            return api;
        }

        static Mock<HttpRequest> MockGetRequest()
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async Task GetStates_LogsRequest()
        {
            // Arrange
            var api = Construct();
            var mockRequest = MockGetRequest();
            var logger = new Mock<ILogger>();

            // Act
            await api.GetStates(mockRequest.Object, logger.Object);

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
        public async Task GetStates_Returns500IfExceptionOccurs()
        {
            // Arrange
            var mockRequest = MockGetRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));
            var logger = new Mock<ILogger>();
            // Mocks
            var statesDao = new Mock<IStateInfoService>();
            statesDao
                .Setup(r => r.GetDecryptedStates())
                .ThrowsAsync(new Exception("Some database error"));

            var api = new StateApi(
                statesDao.Object
            );

            // Act
            var response = await api.GetStates(mockRequest.Object, logger.Object);

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
        public async Task GetStates_ReturnsEmptyListIfNotFound()
        {
            // Arrange
            var mockRequest = MockGetRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));
            var logger = new Mock<ILogger>();
            // Mocks
            var statesDao = new Mock<IStateInfoService>();
            statesDao
                .Setup(r => r.GetDecryptedStates())
                .ReturnsAsync(new List<IState>());

            var api = new StateApi(
                statesDao.Object
            );

            // Act
            var response = await api.GetStates(mockRequest.Object, logger.Object) as JsonResult;

            //Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);

            var resBody = response.Value as StatesInfoResponse;
            Assert.NotNull(resBody);
            Assert.Empty(resBody.Results);
        }

        [Fact]
        public async Task GetStates_ReturnsIfFound()
        {
            // Arrange
            var mockRequest = MockGetRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));
            var logger = new Mock<ILogger>();

            // Mock Dao response
            var matchRecordDao = new Mock<IStateInfoService>();
            matchRecordDao
                .Setup(r => r.GetDecryptedStates())
                .ReturnsAsync(new List<StateInfoDbo>()
                {
                    new StateInfoDbo() { Id = "1", State = "Echo Alpha", StateAbbreviation = "ea" },
                    new StateInfoDbo() { Id = "2", State = "Echo Bravo", StateAbbreviation = "eb" },
                });

            var api = new StateApi(
                matchRecordDao.Object
            );

            // Act
            var response = await api.GetStates(mockRequest.Object, logger.Object) as JsonResult;

            //Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);

            var resBody = response.Value as StatesInfoResponse;
            Assert.NotNull(resBody);
            Assert.Equal(2, resBody.Results.Count());
        }
    }
}
