using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Piipan.Shared.Database;
using Piipan.Shared.Tests.Mocks;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Func.Api;
using Piipan.States.Func.Api.IntegrationTests;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("StateApiTests")]
    public class StateApiIntegrationTests : BaseStateIntegrationTest
    {
        private IDatabaseManager<CoreDbManager> DbManager()
        {
            return new SingleDatabaseManager<CoreDbManager>(ConnectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }
        StateApi Construct()
        {
            SetupServices();
            return (StateApi)GetApi(typeof(StateApi));
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
        public async void GetStates_ReturnsEmptyListIfNotFound()
        {
            // Arrange
            // clear databases
            ClearStates();

            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();

            // Act
            var response = (await api.GetStates(mockRequest.Object, mockLogger) as JsonResult).Value as StatesInfoResponse;

            // Assert
            Assert.Empty(response.Results);
        }

        [Fact]
        public async void GetStates_ReturnsCorrectSchemaIfFound()
        {
            // Arrange
            // clear databases
            ClearStates();

            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();
            // insert into database
            var match = new StateInfoDto()
            {
                Id = "1",
                Email = "EtWidYljr/Pw/KDsutim2XRFsf1XEUCqnIiO3H9m0Ws=",
                State = "Echo Alpha",
                StateAbbreviation = "EA",
                Phone = "PutCWK/BYjM2T3mNl9Skzfl6XQ+TyVcW2Tv6Fs7xZcE=",
                Region = "TEST",
                EmailCc = "SBLhFCtpgWbuZ05nOCt3T3E2WeXpBMyg92jvg4kuoe4="
            };

            var match2 = new StateInfoDto()
            {
                Id = "2",
                Email = "EtWidYljr/Pw/KDsutim2XRFsf1XEUCqnIiO3H9m0Ws=",
                State = "Echo Bravo",
                StateAbbreviation = "EB",
                Phone = "PutCWK/BYjM2T3mNl9Skzfl6XQ+TyVcW2Tv6Fs7xZcE=",
                Region = "TEST",
                EmailCc = "SBLhFCtpgWbuZ05nOCt3T3E2WeXpBMyg92jvg4kuoe4="
            };

            var stateInfoDao = new StateInfoDao(DbManager());
            await stateInfoDao.UpsertState(match);
            await stateInfoDao.UpsertState(match2);

            // Act
            var response = await api.GetStates(mockRequest.Object, mockLogger) as JsonResult;
            var responseRecords = (response.Value as StatesInfoResponse).Results;
            string resString = JsonConvert.SerializeObject(response.Value);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, responseRecords.Count());
            // Assert Participant Data
            var expected = "{\"data\":[{\"id\":\"1\",\"state\":\"Echo Alpha\",\"state_abbreviation\":\"EA\",\"email\":\"ea@test.test\",\"phone\":\"123-555-0105\",\"region\":\"TEST\",\"email_cc\":\"cc@test.test\"},{\"id\":\"2\",\"state\":\"Echo Bravo\",\"state_abbreviation\":\"EB\",\"email\":\"ea@test.test\",\"phone\":\"123-555-0105\",\"region\":\"TEST\",\"email_cc\":\"cc@test.test\"}]}";
            Assert.Equal(expected, resString);
        }
    }
}
