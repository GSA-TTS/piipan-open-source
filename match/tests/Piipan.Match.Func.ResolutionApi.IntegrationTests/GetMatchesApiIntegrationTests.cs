using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Models;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("MatchResolutionApiTests")]
    public class GetMatchesApiIntegrationTests : BaseMatchResIntegrationTests
    {
        GetMatchesApi Construct()
        {
            SetupServices();
            return GetApi<GetMatchesApi>();
        }

        static Mock<HttpRequest> MockGetRequest()
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async void GetMatches_ReturnsEmptyListIfNotFound()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();

            // Act
            var response = (await api.GetMatches(mockRequest.Object, mockLogger) as JsonResult).Value as MatchResListApiResponse;

            // Assert
            Assert.Empty(response.Data);
        }

        [Fact]
        public async void GetMatches_ReturnsCorrectSchemaIfFound()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();
            var matchCreateDate = DateTime.UtcNow;
            // insert into database
            var match = new MatchDbo()
            {
                CreatedAt = matchCreateDate,
                Data = "{\"State\": \"bb\", \"CaseId\": \"GHI\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"JKL\", \"ParticipantClosingDate\": \"2021-02-28\", \"VulnerableIndividual\": true, \"RecentBenefitIssuanceDates\": [{\"start\": \"2021-03-01\", \"end\":\"2021-03-31\"}]}",
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                Input = "{\"CaseId\": \"ABC\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"DEF\"}",
                MatchId = "ABC",
                States = new string[] { "ea", "bb" }
            };

            var match2 = new MatchDbo()
            {
                CreatedAt = matchCreateDate,
                Data = "{\"State\": \"bb\", \"CaseId\": \"123\", \"LdsHash\": \"barbar\", \"ParticipantId\": \"456\", \"ParticipantClosingDate\": \"2021-02-28\", \"VulnerableIndividual\": true, \"RecentBenefitIssuanceDates\": [{\"start\": \"2021-03-01\", \"end\":\"2021-03-31\"}]}",
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                Input = "{\"CaseId\": \"789\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"012\"}",
                MatchId = "DEF",
                States = new string[] { "ea", "bb" }
            };
            Insert(match);
            Insert(match2);

            // Act
            var response = await api.GetMatches(mockRequest.Object, mockLogger) as JsonResult;
            var responseRecords = (response.Value as MatchResListApiResponse).Data;
            var createdDates = responseRecords.Select(n => n.CreatedAt).ToList();
            string resString = JsonConvert.SerializeObject(response.Value);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, responseRecords.Count());
            // Assert Participant Data
            var expected = "{\"data\":[{\"dispositions\":[{\"initial_action_at\":null,\"initial_action_taken\":null,\"invalid_match\":null,\"invalid_match_reason\":null,\"other_reasoning_for_invalid_match\":null,\"final_disposition\":null,\"final_disposition_date\":null,\"vulnerable_individual\":null,\"state\":\"ea\"},{\"initial_action_at\":null,\"initial_action_taken\":null,\"invalid_match\":null,\"invalid_match_reason\":null,\"other_reasoning_for_invalid_match\":null,\"final_disposition\":null,\"final_disposition_date\":null,\"vulnerable_individual\":null,\"state\":\"bb\"}],\"initiator\":\"ea\",\"match_id\":\"ABC\",\"created_at\":" + JsonConvert.SerializeObject(createdDates[0]) + ",\"participants\":[{\"case_id\":\"GHI\",\"participant_closing_date\":\"2021-02-28\",\"participant_id\":\"JKL\",\"recent_benefit_issuance_dates\":[{\"start\":\"2021-03-01\",\"end\":\"2021-03-31\"}],\"state\":\"bb\"},{\"case_id\":\"ABC\",\"participant_closing_date\":null,\"participant_id\":\"DEF\",\"recent_benefit_issuance_dates\":[],\"state\":\"ea\"}],\"states\":[\"ea\",\"bb\"],\"status\":\"open\"}," +
                "{\"dispositions\":[{\"initial_action_at\":null,\"initial_action_taken\":null,\"invalid_match\":null,\"invalid_match_reason\":null,\"other_reasoning_for_invalid_match\":null,\"final_disposition\":null,\"final_disposition_date\":null,\"vulnerable_individual\":null,\"state\":\"ea\"},{\"initial_action_at\":null,\"initial_action_taken\":null,\"invalid_match\":null,\"invalid_match_reason\":null,\"other_reasoning_for_invalid_match\":null,\"final_disposition\":null,\"final_disposition_date\":null,\"vulnerable_individual\":null,\"state\":\"bb\"}],\"initiator\":\"ea\",\"match_id\":\"DEF\",\"created_at\":" + JsonConvert.SerializeObject(createdDates[1]) + ",\"participants\":[{\"case_id\":\"123\",\"participant_closing_date\":\"2021-02-28\",\"participant_id\":\"456\",\"recent_benefit_issuance_dates\":[{\"start\":\"2021-03-01\",\"end\":\"2021-03-31\"}],\"state\":\"bb\"},{\"case_id\":\"789\",\"participant_closing_date\":null,\"participant_id\":\"012\",\"recent_benefit_issuance_dates\":[],\"state\":\"ea\"}],\"states\":[\"ea\",\"bb\"],\"status\":\"open\"}]}";
            Assert.Equal(expected, resString);
            // Assert the created date that is returned is nearly identical to the actual current time
            Assert.True((createdDates[0] - matchCreateDate).Value.TotalMinutes < 1);
            Assert.True((createdDates[1] - matchCreateDate).Value.TotalMinutes < 1);
        }
        // When match res events are added, GetMatch response should update accordingly
        [Fact]
        public async void GetMatches_ShowsUpdatedData()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            var matchId = "ABC";
            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();

            // insert into database
            var match = new MatchDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "bb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // Act
            var response = await api.GetMatches(mockRequest.Object, mockLogger) as JsonResult;

            // Assert first request
            Assert.Equal(200, response.StatusCode);
            var resBody = response.Value as MatchResListApiResponse;
            Assert.Null(resBody.Data.First().Dispositions[0].InvalidMatch);

            // Act again
            // creating an "invalid match" match event results in newly pulled Match request having invalid_match = true
            var mre = new MatchResEventDbo()
            {
                MatchId = matchId,
                ActorState = "ea",
                Actor = "user",
                Delta = "{ \"invalid_match\": true }"
            };
            InsertMatchResEvent(mre);
            var nextResponse = await api.GetMatches(mockRequest.Object, mockLogger) as JsonResult;

            // Assert next request
            var nextResBody = nextResponse.Value as MatchResListApiResponse;
            Assert.Equal(200, nextResponse.StatusCode);
            // now this disposition's invalid flag should be true
            Assert.True(nextResBody.Data.First().Dispositions[0].InvalidMatch);
        }
    }
}