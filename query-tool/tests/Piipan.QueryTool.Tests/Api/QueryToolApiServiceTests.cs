using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Api;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Client.Api;
using Xunit;

namespace Piipan.QueryTool.Tests.Api
{
    public class QueryToolApiServiceTests
    {
        private (Mock<HttpMessageHandler> MockHandler, HttpClient Client) SetupHttpClient(HttpStatusCode statusCode = HttpStatusCode.OK, HttpContent content = null)
        {
            var mockHttp = new Mock<HttpMessageHandler>();
            mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = content
                });

            // Inject the handler or client into your application code
            var client = new HttpClient(mockHttp.Object) { BaseAddress = new System.Uri("https://agency.test.example/") };
            return (mockHttp, client);
        }

        #region GetMatchDetailById
        [Fact]
        public async Task GetMatchDetailById_TestCall_ReturnsApiResponse()
        {
            ApiResponse<MatchResApiResponse> apiResponse = new ApiResponse<MatchResApiResponse>
            {
                IsUnauthorized = true
            };

            var (mockHttp, client) = SetupHttpClient(content: JsonContent.Create(apiResponse));
            QueryToolApiService queryToolApiService = new QueryToolApiService(client);
            var response = await queryToolApiService.GetMatchDetailById("M123456");

            Assert.True(response.IsUnauthorized);

            var httpRequest = mockHttp.Invocations[0].Arguments[0] as HttpRequestMessage;
            Assert.Equal(HttpMethod.Get, httpRequest.Method);
            Assert.Equal("https://agency.test.example/api/match/M123456", httpRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetMatchDetailById_Test500Error()
        {
            // Arrange
            var (mockHttp, client) = SetupHttpClient(HttpStatusCode.InternalServerError);

            QueryToolApiService queryToolApiService = new QueryToolApiService(client);

            // Act
            var response = await queryToolApiService.GetMatchDetailById("M123456");

            // Assert
            Assert.Null(response.Value);
            Assert.Contains(new ServerError("", "Error fetching data. Please try again later."), response.Errors);
        }
        #endregion GetMatchDetailById

        #region GetAllMatchDetails
        [Fact]
        public async Task GetAllMatchDetails_TestCall_ReturnsApiResponse()
        {
            ApiResponse<MatchResListApiResponse> apiResponse = new ApiResponse<MatchResListApiResponse>
            {
                IsUnauthorized = true
            };

            var (mockHttp, client) = SetupHttpClient(content: JsonContent.Create(apiResponse));
            QueryToolApiService queryToolApiService = new QueryToolApiService(client);
            var response = await queryToolApiService.GetAllMatchDetails();

            Assert.True(response.IsUnauthorized);

            var httpRequest = mockHttp.Invocations[0].Arguments[0] as HttpRequestMessage;
            Assert.Equal(HttpMethod.Get, httpRequest.Method);
            Assert.Equal("https://agency.test.example/api/match", httpRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetAllMatchDetails_Test500Error()
        {
            // Arrange
            var (mockHttp, client) = SetupHttpClient(HttpStatusCode.InternalServerError);

            QueryToolApiService queryToolApiService = new QueryToolApiService(client);

            // Act
            var response = await queryToolApiService.GetAllMatchDetails();

            // Assert
            Assert.Null(response.Value);
            Assert.Contains(new ServerError("", "Error fetching data. Please try again later."), response.Errors);
        }
        #endregion GetAllMatchDetails

        #region SaveMatchUpdate
        [Fact]
        public async Task SaveMatchUpdate_TestCall_ReturnsApiResponse()
        {
            ApiResponse<MatchDetailSaveResponse> apiResponse = new ApiResponse<MatchDetailSaveResponse>
            {
                IsUnauthorized = true
            };

            var (mockHttp, client) = SetupHttpClient(content: JsonContent.Create(apiResponse));
            QueryToolApiService queryToolApiService = new QueryToolApiService(client);
            var response = await queryToolApiService.SaveMatchUpdate("M123456", new DispositionModel { VulnerableIndividual = true });

            Assert.True(response.IsUnauthorized);

            var httpRequest = mockHttp.Invocations[0].Arguments[0] as HttpRequestMessage;
            Assert.Equal(HttpMethod.Post, httpRequest.Method);
            Assert.Equal("https://agency.test.example/api/match/M123456", httpRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task SaveMatchUpdate_Test500Error()
        {
            // Arrange
            var (mockHttp, client) = SetupHttpClient(HttpStatusCode.InternalServerError);

            QueryToolApiService queryToolApiService = new QueryToolApiService(client);

            // Act
            var response = await queryToolApiService.SaveMatchUpdate("M123456", new DispositionModel { VulnerableIndividual = true });

            // Assert
            Assert.Null(response.Value);
            Assert.Contains(new ServerError("", "Error saving data. Please try again later."), response.Errors);
        }
        #endregion SaveMatchUpdate

        #region SubmitDuplicateParticipantSearchRequest
        [Fact]
        public async Task SubmitDuplicateParticipantSearchRequest_TestCall_ReturnsApiResponse()
        {
            ApiResponse<OrchMatchResponseData> apiResponse = new ApiResponse<OrchMatchResponseData>
            {
                IsUnauthorized = true
            };

            var (mockHttp, client) = SetupHttpClient(content: JsonContent.Create(apiResponse));
            QueryToolApiService queryToolApiService = new QueryToolApiService(client);
            var response = await queryToolApiService.SubmitDuplicateParticipantSearchRequest(new DuplicateParticipantQuery());

            Assert.True(response.IsUnauthorized);

            var httpRequest = mockHttp.Invocations[0].Arguments[0] as HttpRequestMessage;
            Assert.Equal(HttpMethod.Post, httpRequest.Method);
            Assert.Equal("https://agency.test.example/api/duplicateparticipantsearch", httpRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task SubmitDuplicateParticipantSearchRequest_Test500Error()
        {
            // Arrange
            var (mockHttp, client) = SetupHttpClient(HttpStatusCode.InternalServerError);

            QueryToolApiService queryToolApiService = new QueryToolApiService(client);

            // Act
            var response = await queryToolApiService.SubmitDuplicateParticipantSearchRequest(new DuplicateParticipantQuery());

            // Assert
            Assert.Null(response.Value);
            Assert.Contains(new ServerError("", "Error saving data. Please try again later."), response.Errors);
        }
        #endregion SubmitDuplicateParticipantSearchRequest
    }
}
