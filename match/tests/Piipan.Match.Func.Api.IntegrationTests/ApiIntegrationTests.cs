using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Piipan.Match.Api.Models;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Tests.TestFixtures;
using Xunit;


namespace Piipan.Match.Func.Api.IntegrationTests
{
    public class ApiIntegrationTests : BaseMatchIntegrationTests, IIntegrationTest
    {
        private string InitiatingState = "eb";



        static String JsonBody(object[] records)
        {
            var data = new
            {
                data = records
            };

            return JsonConvert.SerializeObject(data);
        }

        Mock<HttpRequest> MockRequest(string jsonBody)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "From", "a user" },
                    { "Ocp-Apim-Subscription-Name", "sub-name" },
                    { "X-Initiating-State", InitiatingState }
                }));

            return mockRequest;
        }

        public MatchApi Construct()
        {
            SetupServices();
            return (MatchApi)GetApi(typeof(MatchApi));
        }

        [Fact]
        public async Task ApiReturnsMatches()
        {
            // Arrange
            var record = GetDefaultRecord();
            var recordEncrypted = GetEncryptedFullRecord(record);
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, ParticipantId = "participant1", SearchReason = "other" } };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            Insert(recordEncrypted);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);

            var person = resultObject.Data.Results.First();
            Assert.Equal(record.CaseId, person.Matches.First().CaseId);
            Assert.Equal(record.ParticipantId, person.Matches.First().ParticipantId);
            Assert.Equal(state[0], person.Matches.First().State);
            Assert.Equal(recordEncrypted.ParticipantClosingDate, person.Matches.First().ParticipantClosingDate);
            Assert.Equal(recordEncrypted.VulnerableIndividual, person.Matches.First().VulnerableIndividual);
            // serialization
            var match = person.Matches.First();
            var json = JsonConvert.SerializeObject(match);
            Assert.Contains("participant_id", json);
            Assert.Contains("recent_benefit_issuance_dates", json);
        }

        [Fact]
        public async Task ApiReturnsEmptyMatchesArray()
        {
            // Arrange
            var record = GetDefaultRecord();
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, ParticipantId = "participant1", SearchReason = "other" } };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Empty(resultObject.Data.Results[0].Matches);
        }

        [Fact]
        public async Task ApiReturnsEmptyMatchesArrayWhenStateDisabled()
        {
            // Arrange
            var record = GetDefaultRecord();
            var recordEncrypted = GetEncryptedFullRecord(record);
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, ParticipantId = "participant1", SearchReason = "other" } };
            InitiatingState = "ec"; // set to a state that is disabled
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordEncrypted);

            // Assert database is empty prior to the call
            ClearMatchRecords();
            var matchesBefore = GetMatches();
            Assert.Empty(matchesBefore);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Empty(resultObject.Data.Results[0].Matches);

            // Assert a match was actually created even though we got an empty array back
            var matchesAfter = GetMatches();
            Assert.NotEmpty(matchesAfter);
        }

        [Fact]
        public async Task ApiReturnsInlineErrors()
        {
            // Arrange
            var recordA = GetDefaultRecord();
            var recordB = GetDefaultRecord();
            recordB.LdsHash = "foo";
            var logger = Mock.Of<ILogger>();
            var body = new object[] {
                new RequestPerson { LdsHash = recordA.LdsHash, ParticipantId = "participant1", SearchReason = "other" },
                new RequestPerson {LdsHash = recordB.LdsHash, ParticipantId = "participant1", SearchReason = "other" }
            };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);
            Assert.Single(resultObject.Data.Errors);
        }

        [Fact]
        public async Task ApiReturnsExpectedIndices()
        {
            // Arrange
            var recordA = GetDefaultRecord();
            var recordB = GetDefaultRecord();
            var recordEncryptedA = GetEncryptedFullRecord(recordA);

            // lynn,1940-08-01,000-12-3457
            recordB.LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95";
            recordB.ParticipantId = "ParticipantB";
            var recordEncryptedB = GetEncryptedFullRecord(recordB);
            var logger = Mock.Of<ILogger>();
            var body = new object[] {
                new RequestPerson { LdsHash = recordA.LdsHash, ParticipantId = "participant1", SearchReason = "other" },
                new RequestPerson {LdsHash = recordB.LdsHash, ParticipantId = "participant1", SearchReason = "other" }
            };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            InsertUpload();
            ClearParticipants();
            ClearMatchRecords();
            Insert(recordEncryptedA);
            Insert(recordEncryptedB);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            var resultA = resultObject.Data.Results.Find(p => p.Index == 0);
            var resultB = resultObject.Data.Results.Find(p => p.Index == 1);

            AzureAesCryptographyClient cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);

            Assert.Equal(resultA.Matches.First().ParticipantId, recordA.ParticipantId);
            Assert.Equal(resultB.Matches.First().ParticipantId, recordB.ParticipantId);
        }

        [Fact]
        public async Task ApiCreatesMatchRecords()
        {
            // Arrange
            var record = GetDefaultRecord();
            var recordEncrypted = GetEncryptedFullRecord(record);
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, ParticipantId = "participant1", SearchReason = "other" } };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            ClearMatchRecords();
            Insert(recordEncrypted);

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            Assert.Equal(1, CountMatchRecords());
        }

        [Fact]

        public async Task ApiCreatesMatchRecordsWithCorrectValues()
        {
            // Arrange
            var recordA = GetDefaultRecord();
            var recordEncryptedA = GetEncryptedFullRecord(recordA);

            var recordB = GetDefaultRecord();
            // lynn,1940-08-01,000-12-3457
            recordB.LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95";
            recordB.ParticipantId = "ParticipantB";
            var recordEncryptedB = GetEncryptedFullRecord(recordB);
            var logger = Mock.Of<ILogger>();
            var body = new object[] {
                new RequestPerson { LdsHash = recordA.LdsHash, ParticipantId = "participant1", SearchReason = "other" },
                new RequestPerson { LdsHash = recordB.LdsHash, ParticipantId = "participant1", SearchReason = "other" },
            };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            ClearMatchRecords();
            Insert(recordEncryptedA);
            Insert(recordEncryptedB);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.All(resultObject.Data.Results, result =>
            {
                var match = result.Matches.First();
                var record = GetMatchRecord(match.MatchId);

                Assert.Equal(InitiatingState, record.Initiator);
                Assert.True(record.States.SequenceEqual(new string[] { InitiatingState, state[0] }));
                Assert.Equal(match.LdsHash, record.Hash);
                Assert.Equal("ldshash", record.HashType);
            });
        }
    }
}
