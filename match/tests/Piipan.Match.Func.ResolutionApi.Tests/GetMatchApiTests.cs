using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Shared.Http;
using Piipan.States.Core.Service;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class GetMatchApiTests
    {

        static GetMatchApi Construct()
        {
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var mockStatesDao = new Mock<IStateInfoService>();

            var api = new GetMatchApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                mockStatesDao.Object,
                MockMemoryCache().Object
            );
            return api;
        }

        static Mock<HttpRequest> MockGetRequest(string matchId = "foo", string requestLocation = "EA")
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "Ocp-Apim-Subscription-Name", "sub-name" },
                { "X-Request-Location", requestLocation}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        static Mock<IMemoryCache> MockMemoryCache()
        {
            var mockMemoryCache = new Mock<IMemoryCache>();
            object states = new List<StateInfoDbo>
            {
                new StateInfoDbo
                {
                    State = "Echo Alpha",
                    StateAbbreviation = "EA",
                    Region = "Test"
                },
                new StateInfoDbo
                {
                    State = "Echo Bravo",
                    StateAbbreviation = "EB",
                    Region = "Test"
                }
            };
            mockMemoryCache.Setup(n => n.TryGetValue(GetMatchApi.StateInfoCacheName, out states))
                .Returns(true);
            return mockMemoryCache;
        }

        [Fact]
        public async Task GetMatch_LogsRequest()
        {
            // Arrange
            var api = Construct();
            var mockRequest = MockGetRequest();
            var logger = new Mock<ILogger>();

            // Act
            await api.GetMatch(mockRequest.Object, "foo", logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using APIM subscription sub-name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task GetMatch_Returns404IfNotFound()
        {
            // Arrange
            var mockRequest = MockGetRequest();
            var logger = new Mock<ILogger>();
            // Mocks
            var matchRecord = new MatchDbo();
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("not found error"));

            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var mockStatesDao = new Mock<IStateInfoService>();

            var api = new GetMatchApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                mockStatesDao.Object,
                MockMemoryCache().Object
            );

            // Act
            var response = await api.GetMatch(mockRequest.Object, "foo", logger.Object);

            // Assert
            var result = response as NotFoundObjectResult;
            Assert.Equal(404, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("404", errorResponse.Errors[0].Status);
            Assert.Equal("not found", errorResponse.Errors[0].Detail);
            Assert.Contains("NotFoundException", errorResponse.Errors[0].Title);
        }

        [Theory]
        [InlineData("EA", true, "ea", "eb")] // state permissions
        [InlineData("EA", false, "ec", "eb")] // state permissions
        [InlineData("TEST", true, "ec", "eb")] // region permissions
        [InlineData("TEST2", false, "ec", "eb")] // region permissions
        [InlineData("*", true, "ec", "eb")] // national office permissions
        public async Task GetMatch_ReturnsOnlyIfFoundAndLocationMatches(string location, bool expectResult, params string[] states)
        {
            // Arrange
            var mockRequest = MockGetRequest(requestLocation: location);
            var logger = new Mock<ILogger>();

            // Mock Dao response
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo() { States = states, MatchId = "m123456" });
            var mock = new Mock<IMatchResEventDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(new List<IMatchResEvent>());
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            matchResAggregator
                .Setup(r => r.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchDetailsDto()
                {
                    Status = "open"
                });
            var mockStatesDao = new Mock<IStateInfoService>();


            var api = new GetMatchApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                mockStatesDao.Object,
                MockMemoryCache().Object
            );

            // Act
            var response = await api.GetMatch(mockRequest.Object, "m123456", logger.Object);

            if (expectResult)
            {
                var jsonResult = response as JsonResult;
                //Assert
                Assert.NotNull(jsonResult);
                Assert.Equal(200, jsonResult.StatusCode);

                var resBody = jsonResult.Value as MatchResApiResponse;
                Assert.NotNull(resBody);
                Assert.Equal("open", resBody.Data.Status);

            }
            else
            {
                var notFoundResult = response as NotFoundObjectResult;
                //Assert
                Assert.NotNull(notFoundResult);
                Assert.Equal(404, notFoundResult.StatusCode);

                var resBody = notFoundResult.Value as MatchResApiResponse;
                Assert.Null(resBody);

                logger.Verify(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("(NOTAUTHORIZEDMATCH) user (null) did not have access to match id M123456")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ));
            }
        }
    }
}
