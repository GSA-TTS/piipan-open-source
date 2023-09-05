using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Http;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;
using Xunit;

namespace Piipan.States.Func.Api.IntegrationTests
{
    [Collection("StateApiTests")]
    public class UpsertStateInfoApiIntegrationTests : BaseStateIntegrationTest
    {
        UpsertState Construct()
        {
            SetupServices();
            return (UpsertState)GetApi(typeof(UpsertState));
        }

        static StateApi ConstructStateApi()
        {
            Environment.SetEnvironmentVariable("States", "ea");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IStateInfoDao, StateInfoDao>();
            services.AddTransient<IStateInfoService, StateInfoService>();

            services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
            {
                return new SingleDatabaseManager<CoreDbManager>(
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);
            services.RegisterKeyVaultClientServices();

            var provider = services.BuildServiceProvider();

            var cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);

            var api = new StateApi(
                provider.GetService<IStateInfoService>()
            );

            return api;
        }

        static Mock<HttpRequest> MockPostRequest(string jsonBody = "{}")
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
        public async void UpsertReturnsErrorFromDuplicateId()
        {
            // Arrange
            // clear databases
            ClearStates();

            var api = Construct();
            var mockRequest = MockPostRequest("{\"data\":{\"id\":\"100\",\"state\":\"test\",\"state_abbreviation\":\"TT\",\"email\":\"test@test.test\",\"phone\":\"12345\",\"region\":\"TES\",\"email_cc\":\"other@test.test\"}}");
            var mockRequest2 = MockPostRequest("{\"data\":{\"id\":\"100\",\"state2\":\"test\",\"state_abbreviation\":\"TT\",\"email\":\"test@test.test\",\"phone\":\"12345\",\"region\":\"TES\",\"email_cc\":\"other@test.test\"}}");
            var mockLogger = Mock.Of<ILogger>();

            // Act
            await api.Run(mockRequest.Object, mockLogger);
            var response = (await api.Run(mockRequest2.Object, mockLogger) as JsonResult);

            // Assert

            var errorResponse = response.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("500", errorResponse.Errors[0].Status);
        }

        [Fact]
        public async void UpsertInsertsNewState()
        {
            // Arrange
            // clear databases
            ClearStates();
            var mockLogger = Mock.Of<ILogger>();
            var getStatesMockRequest = MockGetRequest();
            var mockRequest = MockPostRequest("{\"data\":{\"id\":\"112\",\"state\":\"test\",\"state_abbreviation\":\"TT\",\"email\":\"test@test.test\",\"phone\":\"12345\",\"region\":\"TES\",\"email_cc\":\"other@test.test\"}}");
            var api = Construct();
            var getStatesApi = ConstructStateApi();

            var getStatesResponseBefore = (await getStatesApi.GetStates(getStatesMockRequest.Object, mockLogger) as JsonResult).Value as StatesInfoResponse;

            // Act
            var response = (await api.Run(mockRequest.Object, mockLogger) as JsonResult);
            var getStatesResponseAfter = (await getStatesApi.GetStates(getStatesMockRequest.Object, mockLogger) as JsonResult).Value as StatesInfoResponse;
            StateInfoDto returnedStateInfoAfter = getStatesResponseAfter.Results.First();

            // Assert
            Assert.Equal(getStatesResponseBefore.Results.Count() + 1, getStatesResponseAfter.Results.Count());
            Assert.Equal("112", returnedStateInfoAfter.Id);
            Assert.Equal("test", returnedStateInfoAfter.State);
            Assert.Equal("TT", returnedStateInfoAfter.StateAbbreviation);
            Assert.Equal("test@test.test", returnedStateInfoAfter.Email);
            Assert.Equal("12345", returnedStateInfoAfter.Phone);
            Assert.Equal("TES", returnedStateInfoAfter.Region);
            Assert.Equal("other@test.test", returnedStateInfoAfter.EmailCc);
        }

        [Fact]
        public async void UpsertStateUpdatesFields()
        {
            // Arrange
            // clear databases
            ClearStates();
            var mockLogger = Mock.Of<ILogger>();
            var getStatesMockRequest = MockGetRequest();
            var mockRequest = MockPostRequest("{\"data\":{\"id\":\"100\",\"state\":\"test\",\"state_abbreviation\":\"TT\",\"email\":\"test@test.test\",\"phone\":\"12345\",\"region\":\"TES\",\"email_cc\":\"other@test.test\"}}");
            var mockRequest2 = MockPostRequest("{\"data\":{\"id\":\"100\",\"state\":\"test\",\"state_abbreviation\":\"T2\",\"email\":\"2@test.test\",\"phone\":\"22222\",\"region\":\"TE2\",\"email_cc\":\"other2@test.test\"}}");
            var api = Construct();
            var getStatesApi = ConstructStateApi();

            // Act
            var response = (await api.Run(mockRequest.Object, mockLogger) as JsonResult);
            var getStatesResponseBefore = (await getStatesApi.GetStates(getStatesMockRequest.Object, mockLogger) as JsonResult).Value as StatesInfoResponse;
            StateInfoDto returnedStateInfoBefore = getStatesResponseBefore.Results.First();
            var response2 = (await api.Run(mockRequest2.Object, mockLogger) as JsonResult);
            var getStatesResponseAfter = (await getStatesApi.GetStates(getStatesMockRequest.Object, mockLogger) as JsonResult).Value as StatesInfoResponse;
            StateInfoDto returnedStateInfoAfter = getStatesResponseAfter.Results.First();

            // Assert
            Assert.Equal("100", returnedStateInfoAfter.Id);
            Assert.Equal("test", returnedStateInfoAfter.State);
            Assert.Equal("T2", returnedStateInfoAfter.StateAbbreviation);
            Assert.Equal("2@test.test", returnedStateInfoAfter.Email);
            Assert.Equal("22222", returnedStateInfoAfter.Phone);
            Assert.Equal("TE2", returnedStateInfoAfter.Region);
            Assert.Equal("other2@test.test", returnedStateInfoAfter.EmailCc);
        }
    }
}
