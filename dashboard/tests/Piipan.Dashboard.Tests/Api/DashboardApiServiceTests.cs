using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Piipan.Dashboard.Client.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Metrics.Api;
using Piipan.Shared.Client.Api;
using Xunit;

namespace Piipan.Dashboard.Tests.Api
{
    public class DashboardApiServiceTests
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
            var client = new HttpClient(mockHttp.Object) { BaseAddress = new System.Uri("https://usda.test.example/") };
            return (mockHttp, client);
        }

        #region GetUploads
        [Fact]
        public async Task GetUploads_TestCall_ReturnsApiResponse()
        {
            // Arrange
            ApiResponse<MatchResApiResponse> apiResponse = new ApiResponse<MatchResApiResponse>
            {
                IsUnauthorized = true
            };

            var (mockHttp, client) = SetupHttpClient(content: JsonContent.Create(apiResponse));
            DashboardApiService dashboardApiService = new DashboardApiService(client);
            var originalFilter = new ParticipantUploadRequestFilter
            {
                State = "EA",
                EndDate = DateTime.Parse("2022-02-01"),
                StartDate = DateTime.Parse("2022-01-01"),
                Status = MatchRecordStatus.Closed,
                HoursOffset = -5,
                Page = 1,
                PerPage = 53
            };

            // Act
            var response = await dashboardApiService.GetUploads(originalFilter);

            // Assert
            Assert.True(response.IsUnauthorized);

            var httpRequest = mockHttp.Invocations[0].Arguments[0] as HttpRequestMessage;
            Assert.Equal(HttpMethod.Get, httpRequest.Method);
            Assert.True(httpRequest.RequestUri.TryReadQueryAs<ParticipantUploadRequestFilter>(out var parsedQueryString));
            Assert.Equal(originalFilter, parsedQueryString);
            Assert.Equal("https://usda.test.example/api/uploads", $"{httpRequest.RequestUri.Scheme}://{httpRequest.RequestUri.Host}{httpRequest.RequestUri.AbsolutePath}");
        }

        [Fact]
        public async Task GetUploads_Test500Error()
        {
            // Arrange
            var (mockHttp, client) = SetupHttpClient(HttpStatusCode.InternalServerError);
            var filter = new ParticipantUploadRequestFilter
            {
                State = "EA",
                EndDate = DateTime.Parse("2022-02-01"),
                StartDate = DateTime.Parse("2022-01-01"),
                Status = MatchRecordStatus.Closed,
                HoursOffset = -5,
                Page = 1,
                PerPage = 53
            };

            DashboardApiService dashboardApiService = new DashboardApiService(client);

            // Act
            var response = await dashboardApiService.GetUploads(filter);

            // Assert
            Assert.Null(response.Value);
            Assert.Contains(new ServerError("", "Error fetching data. Please try again later."), response.Errors);
        }
        #endregion GetUploads

        #region GetUploads
        [Fact]
        public async Task GetUploadStatistics_TestCall_ReturnsApiResponse()
        {
            // Arrange
            ApiResponse<MatchResApiResponse> apiResponse = new ApiResponse<MatchResApiResponse>
            {
                IsUnauthorized = true
            };

            var (mockHttp, client) = SetupHttpClient(content: JsonContent.Create(apiResponse));
            DashboardApiService dashboardApiService = new DashboardApiService(client);

            var statisticsRequest = new ParticipantUploadStatisticsRequest
            {
                EndDate = DateTime.Parse("2022-02-01"),
                StartDate = DateTime.Parse("2022-01-01"),
                HoursOffset = -5,
            };

            // Act
            var response = await dashboardApiService.GetUploadStatistics(statisticsRequest);

            // Assert
            Assert.True(response.IsUnauthorized);

            var httpRequest = mockHttp.Invocations[0].Arguments[0] as HttpRequestMessage;
            Assert.Equal(HttpMethod.Get, httpRequest.Method);

            Assert.True(httpRequest.RequestUri.TryReadQueryAs<ParticipantUploadStatisticsRequest>(out var parsedQueryString));
            Assert.Equal(statisticsRequest, parsedQueryString);
            Assert.Equal("https://usda.test.example/api/uploads/statistics", $"{httpRequest.RequestUri.Scheme}://{httpRequest.RequestUri.Host}{httpRequest.RequestUri.AbsolutePath}");
        }

        [Fact]
        public async Task GetUploadStatistics_Test500Error()
        {
            // Arrange
            var (mockHttp, client) = SetupHttpClient(HttpStatusCode.InternalServerError);
            var filter = new ParticipantUploadStatisticsRequest
            {
                EndDate = DateTime.Parse("2022-02-01"),
                StartDate = DateTime.Parse("2022-01-01"),
                HoursOffset = -5,
            };

            DashboardApiService dashboardApiService = new DashboardApiService(client);

            // Act
            var response = await dashboardApiService.GetUploadStatistics(filter);

            // Assert
            Assert.Null(response.Value);
            Assert.Contains(new ServerError("", "Error fetching data. Please try again later."), response.Errors);
        }
        #endregion GetUploads
    }
}
