using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Metrics.Api;
using Piipan.Shared.Http;
using Piipan.Shared.Parsers;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class AddEventApiTests
    {
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
        public async void AddEvent_LogsRequest()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" },
                    { "From", "foobar"},
                    { "X-Initiating-State", "ea"}
                }));
            var logger = new Mock<ILogger>();

            // Act
            await api.AddEvent(mockRequest.Object, "foo", logger.Object);

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
        public async void AddEvent_Returns404IfNotFound()
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("not found error"));
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            NotFoundObjectResult response = (NotFoundObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ReturnsUnauthorizedIfMatchClosed()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo() 
                { 
                    States = new string[] { "ea", "bc" },
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            matchResAggregator
                .Setup(r => r.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchDetailsDto()
                {
                    Status = "closed"
                });
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfUnrelatedStateActor()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "bc", "de" },
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfSaveDoesntAddAnything()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(new List<IMatchResEvent>());
            matchResEventDao
                .Setup(r => r.AddEvent(
                    It.IsAny<MatchResEventDbo>()
                ))
                .ReturnsAsync(0);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator,
                requestParser,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }");
            var logger = new Mock<ILogger>();

            // Act
            UnprocessableEntityObjectResult response = (UnprocessableEntityObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(422, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Match update save failed. Please try again later.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
            var errorResponse = response.Value as ApiErrorResponse;
            Assert.Equal("Match update save failed. Please try again later.", errorResponse.Errors[0].Detail);
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfDuplicateAction()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true, \"invalid_match_reason\": \"test\" }", // same action as request body
                    ActorState = "ea"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator,
                requestParser,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }"); // same action as delta
            var logger = new Mock<ILogger>();

            // Act
            UnprocessableEntityObjectResult response = (UnprocessableEntityObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(422, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
        }

        /// <summary>
        /// When a match detail event is passed in and doesn't have all of the fields filled out it may not match the last event exactly.
        /// But since it makes no difference to the end result, it should be treated as a duplicate action and rejected.
        /// </summary>
        [Fact]
        public async void AddEvent_ReturnsErrorIfDuplicateAction_EvenIfNulls()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true, \"invalid_match_reason\": \"test\", \"vulnerable_individual\": null }", // same action as request body plus a null vulnerable individual
                    ActorState = "ea"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator,
                requestParser,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }"); // same action as delta minus vulnerable individual
            var logger = new Mock<ILogger>();

            // Act
            UnprocessableEntityObjectResult response = (UnprocessableEntityObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(422, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
        }

        /// <summary>
        /// When there are multiple match detail events for multiple states, previously the algorithm would get confused and compare the last action
        /// even if the states were different. We should compare the entire match object, not just the last event.
        /// </summary>
        [Fact]
        public async void AddEvent_ReturnsErrorIfDuplicateAction_EvenIfAnotherEventForDifferentStateAfter()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true, \"invalid_match_reason\": \"test\" }", // same action as request body
                    ActorState = "ea"
                },
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": false }", // A different action for another state saved after the EA one
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator,
                requestParser,
                publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }"); // same action as the first event's delta
            var logger = new Mock<ILogger>();

            // Act
            UnprocessableEntityObjectResult response = (UnprocessableEntityObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(422, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfRequestBodyInvalid()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": \"foo\" } }");
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "bar", logger.Object));

            // Assert
            Assert.Equal(400, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfRequestNotParsed()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("foobar");
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "bar", logger.Object));

            // Assert
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_InsertsEventOnSuccess()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                .Returns(Task.CompletedTask);

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"vulnerable_individual\": true } }");
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(It.IsAny<MatchResEventDbo>()), Times.Once());
            matchResEventDao.Verify(mock => mock.AddEvent(It.Is<MatchResEventDbo>(m => m.ActorState == "ea")), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo")), Times.Once());
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ChecksIfShouldBeClosedOnSuccess()
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    CreatedAt = DateTime.Parse("2022-07-01"),
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                }
            };
            var eventsAfterUpdate = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\"}",
                    ActorState = "eb"
                },
                 new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "ea"
                },
                 new MatchResEventDbo()
                 {
                     Delta = "{ \"status\": \"closed\" }"
                 }
            };
            matchResEventDao
                .SetupSequence(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events)
                .ReturnsAsync(eventsAfterUpdate);

            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                  .Returns(Task.CompletedTask);

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"2022-07-20T00:00:01\" } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo" && m.Status == "closed")), Times.Once());

            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ChecksIfShouldBeClosedOnSuccess_WithOneInvalid()
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-07-01"),
                    MatchId = "foo",
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var eventsBeforeUpdate = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                }
            };
            var eventsAfterUpdate = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                },
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true, \"invalid_match_reason\": \"test\" }",
                    ActorState = "ea"
                },
                new MatchResEventDbo {
                    Delta = "{ \"status\": \"closed\" }"
                }
            };

            matchResEventDao
                .SetupSequence(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(eventsBeforeUpdate)
                .ReturnsAsync(eventsAfterUpdate);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                 .Returns(Task.CompletedTask);

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Once());
            Assert.Equal(200, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo" && m.Status == "closed")), Times.Once());
        }

        [Fact]
        public async void AddEvent_ChecksIfShouldBeClosedOnSuccess_WithBothInvalid()
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    MatchId = "foo",
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-07-01"),
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true, \"invalid_match_reason\": \"test\" }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);

            // we were mocking the MatchDetailsAggregator, but it's unneccessary and adds a maintenance headache.
            // We should just use the actual aggregator since it doesn't make any API/DB calls nor are we verifying any of the calls.
            var matchResAggregator = new MatchDetailsAggregator();

            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                .Returns(Task.CompletedTask);

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo")), Times.Once());

            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_CheckIfFinalDispositionDateError()
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-08-01"), // Created At date > Final Disposition Date
                    Data = "{}",
                    Input = "{}"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            matchResAggregator
                .Setup(r => r.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchDetailsDto()
                {
                    Status = "open"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"2022-07-20T00:00:01\" } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Never());
            Assert.Equal(400, response.StatusCode);
        }
        [Theory]
        [InlineData("1/1/1888")] // start Date
        [InlineData("1/1/2051")] // start Date
        public async void AddEvent_CheckDateValidation_Error_Initial_ActionDAte( string initialActionAt)
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-08-01"), // Created At date > Final Disposition Date
                    Data = "{}",
                    Input = "{}"
                });
            string delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"@@@\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }";
            string mockRequestData = "{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"@@@\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"2022-07-20T00:00:01\" } }";
            mockRequestData = mockRequestData.Replace("@@@", initialActionAt);
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = delta.Replace("@@@",initialActionAt),
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            matchResAggregator
                .Setup(r => r.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchDetailsDto()
                {
                    Status = "open"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest(mockRequestData); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Never());
              Assert.Equal("Inital Action Date must have a year between 1900 and 2050", ((Piipan.Shared.Http.ApiErrorResponse)response.Value).Errors[0].Detail);
        }
        [Theory]
        [InlineData("1/1/1888")] // start Date
        [InlineData("1/1/2051")] // start Date
        public async void AddEvent_CheckDateValidation_Error_DispositionDate(string finalDispositionDate)
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-08-01"), // Created At date > Final Disposition Date
                    Data = "{}",
                    Input = "{}"
                });
            string delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\\\" ,  \"final_disposition\": \"foo\", \"final_disposition_date\": \"@@@\" }";
            string mockRequestData = "{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"@@@\" } }";
            mockRequestData = mockRequestData.Replace("@@@", finalDispositionDate);
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = delta.Replace("@@@",finalDispositionDate),
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            matchResAggregator
                .Setup(r => r.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchDetailsDto()
                {
                    Status = "open"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest(mockRequestData); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Never());
            Assert.Equal("Final Disposition Date must have a year between 1900 and 2050", ((Piipan.Shared.Http.ApiErrorResponse)response.Value).Errors[0].Detail);
        }
        [Theory]
        [InlineData("1/1/1888")] // start Date
        [InlineData("1/1/2051")] // start Date
        public async void AddEvent_CheckDateValidation_Error_BenefitsEndDate(string finalDispositionDate)
        {
            var matchRecordDao = new Mock<IMatchDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-08-01"), // Created At date > Final Disposition Date
                    Data = "{}",
                    Input = "{}"
                });
            string delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\\\" ,  \"final_disposition\": \"Benefits Terminated\", \"final_disposition_date\": \"@@@\" }";
            string mockRequestData = "{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"Benefits Terminated\", \"final_disposition_date\": \"@@@\" } }";
            mockRequestData = mockRequestData.Replace("@@@", finalDispositionDate);
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = delta.Replace("@@@",finalDispositionDate),
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            matchResAggregator
                .Setup(r => r.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchDetailsDto()
                {
                    Status = "open"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object
            );
            var mockRequest = MockRequest(mockRequestData); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Never());
            Assert.Equal("Benefits End Date must have a year between 1900 and 2050", ((Piipan.Shared.Http.ApiErrorResponse)response.Value).Errors[0].Detail);
        }
    }
}