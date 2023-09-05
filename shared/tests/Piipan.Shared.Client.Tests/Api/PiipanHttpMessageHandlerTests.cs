using System.Net;
using System.Net.Http.Json;
using System.Text;
using Moq;
using Moq.Protected;
using Piipan.Components.Modals;
using Piipan.Shared.Client.Api;
using Xunit;

namespace Piipan.Shared.Client.Tests.Api
{
    public class PiipanHttpMessageHandlerTests
    {
        private Mock<PiipanHttpMessageHandler> CreateHandler(Mock<TestPiipanNavigationManager>? navManager = null,
            Mock<IModalManager>? modalManager = null)
        {
            navManager ??= new Mock<TestPiipanNavigationManager>();
            modalManager ??= new Mock<IModalManager>();
            Mock<PiipanHttpMessageHandler> mockMessageHandler = new Mock<PiipanHttpMessageHandler>(navManager.Object, modalManager.Object);
            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).CallBase();
            return mockMessageHandler;
        }

        private readonly HttpResponseMessage ServiceUnavailableResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
        };

        [Fact]
        public async Task SendAsync_ReturnsJsonObject_AsExpected()
        {
            // Arrange 
            ApiResponse<int> dummyResponse = new ApiResponse<int>
            {
                Value = 2
            };

            var mockMessageHandler = CreateHandler();
            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("BaseSendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = JsonContent.Create(dummyResponse),
                StatusCode = HttpStatusCode.OK,
            });

            HttpClient httpClient = new HttpClient(mockMessageHandler.Object);

            // Act
            var response = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = new Uri("https://test.example") });

            // Assert
            var testResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            Assert.Equal(2, testResponse!.Value);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            mockMessageHandler.Protected().Verify("BaseSendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [Theory]
        public async Task SendAsync_Retries3Times_AsExpected(HttpStatusCode statusCode)
        {
            // Arrange 
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };
            var mockMessageHandler = CreateHandler();
            mockMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("BaseSendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse)
                .ReturnsAsync(httpResponse)
                .ReturnsAsync(httpResponse)
                .ReturnsAsync(httpResponse);

            HttpClient httpClient = new HttpClient(mockMessageHandler.Object);

            // Act
            var response = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = new Uri("https://test.example") });

            // Assert
            Assert.Equal(statusCode, response.StatusCode);
            mockMessageHandler.Protected().Verify("BaseSendAsync", Times.Exactly(4), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_ReturnsJson_IfRetrySucceeds()
        {
            // Arrange 
            ApiResponse<int> dummyResponse = new ApiResponse<int>
            {
                Value = 2
            };

            var mockMessageHandler = CreateHandler();
            mockMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("BaseSendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(ServiceUnavailableResponse)
                .ReturnsAsync(ServiceUnavailableResponse)
                .ReturnsAsync(ServiceUnavailableResponse)
                .ReturnsAsync(new HttpResponseMessage()
                {
                    Content = JsonContent.Create(dummyResponse),
                    StatusCode = HttpStatusCode.OK,
                });

            HttpClient httpClient = new HttpClient(mockMessageHandler.Object);

            // Act
            var response = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = new Uri("https://test.example") });

            // Assert
            var testResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            Assert.Equal(2, testResponse!.Value);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            mockMessageHandler.Protected().Verify("BaseSendAsync", Times.Exactly(4), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [Theory]
        public async Task SendAsync_RefreshesPage_IfForbiddenOrUnauthorized(HttpStatusCode statusCode)
        {
            // Arrange 
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };
            var mockNavManager = new Mock<TestPiipanNavigationManager>();
            var mockMessageHandler = CreateHandler(mockNavManager);
            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("BaseSendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            HttpClient httpClient = new HttpClient(mockMessageHandler.Object);

            // Act
            var response = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = new Uri("https://test.example") });

            // Assert
            Assert.Equal(statusCode, response.StatusCode);
            mockMessageHandler.Protected().Verify("BaseSendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mockNavManager.Protected().Verify("NavigateToCore", Times.Once(), "https://test.example/", true);
        }

        [Fact]
        public async Task SendAsync_RefreshesPage_IfWeCantDetermineRedirect()
        {
            // Arrange 
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent("<html></html>", Encoding.ASCII, "text/html"),
                StatusCode = HttpStatusCode.OK
            };
            var mockNavManager = new Mock<TestPiipanNavigationManager>();
            var mockMessageHandler = CreateHandler(mockNavManager);
            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("BaseSendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            HttpClient httpClient = new HttpClient(mockMessageHandler.Object);

            // Act
            var response = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = new Uri("https://test.example") });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            mockMessageHandler.Protected().Verify("BaseSendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mockNavManager.Protected().Verify("NavigateToCore", Times.Once(), "https://test.example/", true);
        }

        [Fact]
        public async Task SendAsync_ShowsRedirectModal_IfWeCanDetermineRedirect()
        {
            // Arrange 
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent("<html><body><script>window.location.replace('https://test.auth.example')</script></body></html>", Encoding.ASCII, "text/html"),
                StatusCode = HttpStatusCode.OK
            };
            var mockNavManager = new Mock<TestPiipanNavigationManager>();
            var mockModalManager = new Mock<IModalManager>();
            var mockMessageHandler = CreateHandler(mockNavManager, mockModalManager);
            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("BaseSendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            HttpClient httpClient = new HttpClient(mockMessageHandler.Object);

            // Act
            var response = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = new Uri("https://test.example") });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            mockMessageHandler.Protected().Verify("BaseSendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            mockNavManager.Protected().Verify("NavigateToCore", Times.Never(), "https://test.example/", true);
            mockModalManager.Verify(n => n.Show(
                It.Is<RedirectToLoginModal>(m => m.RedirectLocation == "https://test.auth.example"),
                It.Is<ModalInfo>(m => m.ForceAction)), Times.Once());
        }
    }
}