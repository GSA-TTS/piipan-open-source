using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Models;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("MatchResolutionApiTests")]
    public class AddEventApiIntegrationTests : BaseMatchResIntegrationTests
    {
        AddEventApi Construct()
        {
            SetupServices();
            return GetApi<AddEventApi>();
        }

        static Mock<HttpRequest> MockRequest(string jsonBody = "{}")
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
        public async void AddEvent_ReturnsErrorIfNotRelatedState()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ec",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ec", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}"
            };
            Insert(match);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            var response = await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object) as UnauthorizedResult;

            // Assert
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfClosed()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // insert an event into the database
            var matchResEvent = new MatchResEventDbo()
            {
                MatchId = matchId,
                Actor = "system",
                Delta = "{ \"status\": \"closed\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_SuccessInsertsEvent()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object));
            var events = await GetEvents(matchId);
            var lastEvent = events.Last();
            // Assert
            Assert.Single(events);
            Assert.Equal(matchId, lastEvent.MatchId);
            Assert.Equal("ea", lastEvent.ActorState);
            Assert.Equal("{\"invalid_match\": true, \"invalid_match_reason\": \"test\"}", lastEvent.Delta);
            Assert.Equal(200, response.StatusCode);

        }

        [Fact]
        public async void AddEvent_SuccessInsertsEvent_WhenMixedCase()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true, \"invalid_match_reason\": \"test\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            string matchIdMixedCase = "aBCdEfG";
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, matchIdMixedCase, mockLogger.Object));
            var events = await GetEvents(matchId);
            var lastEvent = events.Last();
            // Assert
            Assert.Single(events);
            Assert.Equal(matchId, lastEvent.MatchId);
            Assert.Equal("ea", lastEvent.ActorState);
            Assert.Equal("{\"invalid_match\": true, \"invalid_match_reason\": \"test\"}", lastEvent.Delta);
            Assert.Equal(200, response.StatusCode);

        }

        [Fact]
        public async void AddEvent_SuccessInsertsClosedEventIfClosed()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();
            // insert a match into db
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // insert final disposition event into db
            var matchResEvent = new MatchResEventDbo()
            {
                MatchId = matchId,
                Actor = "user",
                ActorState = "eb",
                Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"" + System.DateTime.UtcNow.AddDays(2).ToString("s") + "\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object));
            var events = await GetEvents(matchId);
            var lastEvent = events.Last();
            // Assert
            Assert.Equal(3, events.Count());
            Assert.Equal(matchId, lastEvent.MatchId);
            Assert.Equal(api.SystemActor, lastEvent.Actor);
            Assert.Equal(api.ClosedDelta, lastEvent.Delta);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_AddsEvent_OnlyPropertiesPassedIn()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();
            // insert a match into db
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // insert final disposition event into db
            var matchResEvent = new MatchResEventDbo()
            {
                MatchId = matchId,
                Actor = "user",
                ActorState = "eb",
                Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest1 = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"" + System.DateTime.UtcNow.AddDays(2).ToString("s") + "\" } }");
            var mockRequest2 = MockRequest("{ \"data\": { \"initial_action_taken\": \"Client Verified Moved Out of State\", \"initial_action_at\": \"2022-07-20T00:00:02\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            OkResult response1 = (OkResult)(await api.AddEvent(mockRequest1.Object, matchId, mockLogger.Object));
            Assert.Equal(200, response1.StatusCode);

            OkResult response2 = (OkResult)(await api.AddEvent(mockRequest2.Object, matchId, mockLogger.Object));
            Assert.Equal(200, response2.StatusCode);

            var getMatchApi = GetApi<GetMatchApi>();
            var matchResponse = await getMatchApi.GetMatch(GetMatchApiIntegrationTests.MockGetRequest(matchId, "EA").Object, matchId, mockLogger.Object) as JsonResult;
            var eaDisposition = (matchResponse.Value as MatchResApiResponse).Data.Dispositions.FirstOrDefault(n => n.State == "ea");

            // Assert
            Assert.Equal("bar", eaDisposition.FinalDisposition);
            Assert.NotNull(eaDisposition.FinalDispositionDate);
            Assert.Equal("Client Verified Moved Out of State", eaDisposition.InitialActionTaken);
        }

        [Fact]
        public async void AddEvent_AddsEvent_OnlyPropertiesPassedIn_EvenNulls()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();
            // insert a match into db
            var matchId = "ABCDEFG";
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // insert final disposition event into db
            var matchResEvent = new MatchResEventDbo()
            {
                MatchId = matchId,
                Actor = "user",
                ActorState = "eb",
                Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest1 = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"" + System.DateTime.UtcNow.AddDays(2).ToString("s") + "\" } }");
            var mockRequest2 = MockRequest("{ \"data\": { \"initial_action_taken\": \"Client Verified Moved Out of State\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": null, \"final_disposition_date\": null } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            OkResult response1 = (OkResult)(await api.AddEvent(mockRequest1.Object, matchId, mockLogger.Object));
            Assert.Equal(200, response1.StatusCode);

            OkResult response2 = (OkResult)(await api.AddEvent(mockRequest2.Object, matchId, mockLogger.Object));
            Assert.Equal(200, response2.StatusCode);

            var getMatchApi = GetApi<GetMatchApi>();
            var matchResponse = await getMatchApi.GetMatch(GetMatchApiIntegrationTests.MockGetRequest(matchId, "EA").Object, matchId, mockLogger.Object) as JsonResult;
            var eaDisposition = (matchResponse.Value as MatchResApiResponse).Data.Dispositions.FirstOrDefault(n => n.State == "ea");

            // Assert
            Assert.Null(eaDisposition.FinalDisposition);
            Assert.Null(eaDisposition.FinalDispositionDate);
            Assert.Equal("Client Verified Moved Out of State", eaDisposition.InitialActionTaken);
        }
    }
}