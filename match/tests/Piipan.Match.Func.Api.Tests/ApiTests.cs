using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Http;
using Piipan.Shared.Parsers;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Match.Func.Api.Tests
{
    public class ApiTests
    {
        static ParticipantMatch FullRecord()
        {
            return new ParticipantMatch
            {
                CaseId = "CaseIdExample",
                ParticipantClosingDate = new DateTime(1970, 1, 31),
                RecentBenefitIssuanceDates = new List<DateRange>() {
                  new DateRange(new DateTime(2021, 5, 31) , new DateTime(2021,6,15)),
                  new DateRange(new DateTime(2021, 4, 30) , new DateTime(2021,5,1)),
                  new DateRange(new DateTime(2021, 3, 31) , new DateTime(2021,5,15))
                },
                VulnerableIndividual = true
            };
        }

        static OrchMatchRequest FullRequest()
        {
            return new OrchMatchRequest
            {
                Data = new List<RequestPerson>() {
                    new RequestPerson
                    {
                        // farrington,1931-10-13,000-12-3456
                        LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
                    }
                }
            };
        }

        static OrchMatchRequest FullRequestMultiple()
        {
            return new OrchMatchRequest
            {
                Data = new List<RequestPerson>() {
                    new RequestPerson
                    {
                        // farrington,1931-10-13,000-12-3456
                        LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
                    },
                    new RequestPerson
                    {
                        // lynn,1940-08-01,000-12-3457
                        LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95"
                    }
                }
            };
        }

        static OrchMatchRequest OverMaxRequest()
        {
            var list = new List<RequestPerson>();
            for (int i = 0; i < 51; i++)
            {
                list.Add(new RequestPerson
                {
                    // farrington,1931-10-13,000-12-3456
                    LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
                });
            }
            return new OrchMatchRequest { Data = list };
        }

        static OrchMatchResult StateResponse()
        {
            var stateResponse = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch> { FullRecord() }
            };
            return stateResponse;
        }

        static String JsonBody(string json)
        {
            var data = new
            {
                data = JsonConvert.DeserializeObject(json)
            };

            return JsonConvert.SerializeObject(data);
        }

        static Mock<HttpRequest> MockRequest(string jsonBody, string initiatingState = "ea")
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
                { "X-Initiating-State", initiatingState}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        static Mock<IMemoryCache> MockMemoryCache()
        {
            var mockMemoryCache = new Mock<IMemoryCache>();
            object states = new string[] { "ea", "eb" };
            mockMemoryCache.Setup(n => n.TryGetValue("EnabledStates", out states))
                .Returns(true);
            return mockMemoryCache;
        }

        static HttpResponseMessage MockResponse(System.Net.HttpStatusCode statusCode, string body)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        static Mock<HttpMessageHandler> MockMessageHandler(List<HttpResponseMessage> responses)
        {
            var responseQueue = new Queue<HttpResponseMessage>();
            foreach (HttpResponseMessage response in responses)
            {
                responseQueue.Enqueue(response);
            }
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseQueue.Dequeue)
                .Verifiable();

            return mockHttpMessageHandler;
        }

        static MatchApi Construct()
        {
            var matchService = new Mock<IMatchSearchApi>();
            var requestParser = new OrchMatchRequestParser(
                new OrchMatchRequestValidator(),
                Mock.Of<ILogger<OrchMatchRequestParser>>()
            );
            var matchEventService = new Mock<IMatchEventService>();
            var api = new MatchApi(
                matchService.Object,
                requestParser,
                matchEventService.Object,
                MockMemoryCache().Object);

            return api;
        }

        static MatchApi ConstructMocked(Mock<HttpMessageHandler> handler)
        {
            var matchService = new Mock<IMatchSearchApi>();
            var requestParser = new OrchMatchRequestParser(
                new OrchMatchRequestValidator(),
                Mock.Of<ILogger<OrchMatchRequestParser>>()
            );
            var matchEventService = new Mock<IMatchEventService>();

            var api = new MatchApi(
                matchService.Object,
                requestParser,
                matchEventService.Object,
                MockMemoryCache().Object);

            return api;
        }

        ////
        // Tests
        ////

        [Fact]
        public async void ParserExceptionResultsInBadRequest()
        {
            // Arrange
            var matchService = Mock.Of<IMatchSearchApi>();
            var requestParser = new Mock<IStreamParser<OrchMatchRequest>>();
            var logger = Mock.Of<ILogger>();
            var matchEventService = Mock.Of<IMatchEventService>();
            var mockRequest = MockRequest("");
            requestParser
                .Setup(m => m.Parse(It.IsAny<Stream>()))
                .ThrowsAsync(new StreamParserException("failed to parse"));

            var api = new MatchApi(
                matchService,
                requestParser.Object,
                matchEventService,
                MockMemoryCache().Object);

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("400", errorResponse.Errors[0].Status);
            Assert.Equal("failed to parse", errorResponse.Errors[0].Detail);
            Assert.Contains("StreamParserException", errorResponse.Errors[0].Title);
        }

        [Fact]
        public async void ValidationExceptionResultsInBadRequest()
        {
            // Arrange
            var matchService = Mock.Of<IMatchSearchApi>();
            var requestParser = new Mock<IStreamParser<OrchMatchRequest>>();
            var logger = Mock.Of<ILogger>();
            var matchEventService = Mock.Of<IMatchEventService>();
            var mockRequest = MockRequest("");

            requestParser
                .Setup(m => m.Parse(It.IsAny<Stream>()))
                .ThrowsAsync(new ValidationException("failed to validate", new List<ValidationFailure>
                {
                    new ValidationFailure("property", "property missing")
                }));

            var api = new MatchApi(
                matchService,
                requestParser.Object,
                matchEventService,
                MockMemoryCache().Object);

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("400", errorResponse.Errors[0].Status);
            Assert.Equal("property missing", errorResponse.Errors[0].Detail);
        }

        [Fact]
        public async void MissingInitiatingStateHeaderResultsInBadRequest()
        {
            // Arrange
            var matchService = Mock.Of<IMatchSearchApi>();
            var requestParser = new Mock<IStreamParser<OrchMatchRequest>>();
            var logger = Mock.Of<ILogger>();
            var matchEventService = Mock.Of<IMatchEventService>();
            var mockRequest = MockRequest("");
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues> { }));

            var api = new MatchApi(
                matchService,
                requestParser.Object,
                matchEventService,
                MockMemoryCache().Object);

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as BadRequestObjectResult;
            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("400", errorResponse.Errors[0].Status);
            Assert.Contains("missing required header: X-Initiating-State", errorResponse.Errors[0].Detail);
        }

        // Whole thing blows up and returns a top-level error
        [Fact]
        public async void ReturnsInternalServerError()
        {
            // Arrange
            var api = Construct();
            Mock<HttpRequest> mockRequest = MockRequest("foobar");
            var logger = new Mock<ILogger>();

            // Set up first log to throw an exception
            // How to mock LogInformation: https://stackoverflow.com/a/58413842
            logger.SetupSequence(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Throws(new Exception("example message"));

            // Act
            var response = await api.Find(mockRequest.Object, logger.Object);
            var result = response as JsonResult;
            var resBody = result.Value as ApiErrorResponse;
            var error = resBody.Errors[0];

            // Assert
            Assert.Equal(500, result.StatusCode);
            Assert.NotEmpty(resBody.Errors);
            Assert.Equal("500", error.Status);
            Assert.NotNull(error.Title);
            Assert.NotNull(error.Detail);
        }

        [Fact]
        public async Task LogsApimSubscriptionIfPresent()
        {
            // Arrange
            var api = Construct();
            var mockRequest = MockRequest("foobar");
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));

            var logger = new Mock<ILogger>();

            // Act
            await api.Find(mockRequest.Object, logger.Object);

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
        public async Task LogsNoApimSubscriptionIfNotPresent()
        {
            // Arrange
            var api = Construct();
            var mockRequest = MockRequest("foobar");
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                }));

            var logger = new Mock<ILogger>();

            // Act
            await api.Find(mockRequest.Object, logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No APIM Subscription found. Requested from Web App.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task Returns()
        {
            // Arrange
            var response = new OrchMatchResponse
            {
                Data = new OrchMatchResponseData
                {
                    Results = new List<OrchMatchResult>
                    {
                        new OrchMatchResult
                        {
                            Index = 0,
                            Matches = new ParticipantMatch[] { new ParticipantMatch { LdsHash = "asdf", State = "ea" } }
                        }
                    },
                    Errors = new List<OrchMatchError>
                    {
                        new OrchMatchError
                        {
                            Index = 1,
                            Code = "code",
                            Title = "title",
                            Detail = "detail"
                        }
                    }
                }
            };

            var matchService = new Mock<IMatchSearchApi>();
            matchService
                .Setup(m => m.FindAllMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            var requestParser = new Mock<IStreamParser<OrchMatchRequest>>();
            var logger = Mock.Of<ILogger>();
            var matchEventService = new Mock<IMatchEventService>();
            matchEventService
                .Setup(r => r.ResolveMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<OrchMatchResponse>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(response);
            var mockRequest = MockRequest("");

            var api = new MatchApi(
                matchService.Object,
                requestParser.Object,
                matchEventService.Object,
                MockMemoryCache().Object);

            // Act
            var apiResponse = (await api.Find(mockRequest.Object, logger)) as JsonResult;

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(200, apiResponse.StatusCode);

            var matchResponse = apiResponse.Value as OrchMatchResponse;
            Assert.NotNull(matchResponse);
            Assert.Equal(response, matchResponse);
            Assert.NotEmpty(response.Data.Results[0].Matches);
        }

        [Fact]
        public async Task ReturnsNothingWhenInitiatingStateDisabled()
        {
            // Arrange
            var response = new OrchMatchResponse
            {
                Data = new OrchMatchResponseData
                {
                    Results = new List<OrchMatchResult>
                    {
                        new OrchMatchResult
                        {
                            Index = 0,
                            Matches = new ParticipantMatch[] { new ParticipantMatch { LdsHash = "asdf", State = "ea" } }
                        }
                    },
                    Errors = new List<OrchMatchError>
                    {
                        new OrchMatchError
                        {
                            Index = 1,
                            Code = "code",
                            Title = "title",
                            Detail = "detail"
                        }
                    }
                }
            };
            var responseWithoutMatches = new OrchMatchResponse
            {
                Data = new OrchMatchResponseData
                {
                    Results = new List<OrchMatchResult>
                    {
                        new OrchMatchResult
                        {
                            Index = 0,
                            Matches = new ParticipantMatch[0]
                        }
                    }
                }
            };

            var matchService = new Mock<IMatchSearchApi>();
            matchService
                .Setup(m => m.FindAllMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            var requestParser = new Mock<IStreamParser<OrchMatchRequest>>();
            var logger = Mock.Of<ILogger>();
            var matchEventService = new Mock<IMatchEventService>();
            matchEventService
                .Setup(r => r.ResolveMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<OrchMatchResponse>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(response);
            var mockRequest = MockRequest("", "ec");

            var api = new MatchApi(
                matchService.Object,
                requestParser.Object,
                matchEventService.Object,
                MockMemoryCache().Object);

            // Act
            var apiResponse = (await api.Find(mockRequest.Object, logger)) as JsonResult;

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(200, apiResponse.StatusCode);

            var matchResponse = apiResponse.Value as OrchMatchResponse;
            Assert.NotNull(matchResponse);
            Assert.Empty(matchResponse.Data.Results[0].Matches);
        }

        [Fact]
        public async Task ReturnsNothingWhenMatchingStateDisabled()
        {
            // Arrange
            var response = new OrchMatchResponse
            {
                Data = new OrchMatchResponseData
                {
                    Results = new List<OrchMatchResult>
                    {
                        new OrchMatchResult
                        {
                            Index = 0,
                            Matches = new ParticipantMatch[] { new ParticipantMatch { LdsHash = "asdf", State = "ec" } }
                        }
                    }
                }
            };
            var responseWithoutMatches = new OrchMatchResponse
            {
                Data = new OrchMatchResponseData
                {
                    Results = new List<OrchMatchResult>
                    {
                        new OrchMatchResult
                        {
                            Index = 0,
                            Matches = new ParticipantMatch[0]
                        }
                    }
                }
            };

            var matchService = new Mock<IMatchSearchApi>();
            matchService
                .Setup(m => m.FindAllMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            var requestParser = new Mock<IStreamParser<OrchMatchRequest>>();
            var logger = Mock.Of<ILogger>();
            var matchEventService = new Mock<IMatchEventService>();
            matchEventService
                .Setup(r => r.ResolveMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<OrchMatchResponse>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(response);
            var mockRequest = MockRequest("", "ea");

            var api = new MatchApi(
                matchService.Object,
                requestParser.Object,
                matchEventService.Object,
                MockMemoryCache().Object);

            // Act
            var apiResponse = (await api.Find(mockRequest.Object, logger)) as JsonResult;

            // Assert
            Assert.NotNull(apiResponse);
            Assert.Equal(200, apiResponse.StatusCode);

            var matchResponse = apiResponse.Value as OrchMatchResponse;
            Assert.NotNull(matchResponse);
            Assert.Empty(matchResponse.Data.Results[0].Matches);
        }

        [Fact]
        public void TestStartUp_ShouldCreateObject()
        {
            //Arrange
            var functionsHostBuilder = new Mock<IFunctionsHostBuilder>();

            //Act
            var startUp = new Startup();

            //Assert
            Assert.NotNull(startUp);
            Assert.Contains("Configure", typeof(Startup).GetMethods().Select(c => c.Name));
        }

        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            Environment.SetEnvironmentVariable(Startup.ParticipantsDatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");

            Environment.SetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString,
                "Server=server;Database=dbCollaboration;Port=5432;User Id=postgres;Password={password};");

            var tester = new DependencyTester()
                .Register<MatchApi>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}
