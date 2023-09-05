using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using Piipan.Shared.Client.Api;
using Xunit;

namespace Piipan.Shared.Client.Tests.Api
{
    public class PiipanApiServiceTests
    {
        private const string BaseApiAddress = "https://test.example";
        private const string ApiPath = "/api/test";

        /// <summary>
        /// Using a record here to make the comparison easier when making sure the body was set
        /// </summary>
        private record TestRecord(DateTime start, DateTime end);
        private readonly TestRecord ItemToSave = new TestRecord(DateTime.Now, DateTime.Now.AddDays(1));
        private readonly ApiResponse<int> DummyResponse = new() { Value = 1 };

        Mock<HttpMessageHandler>? httpMessageHandler;

        private PiipanApiService CreateApiService(Action<Moq.Language.Flow.ISetup<HttpMessageHandler, Task<HttpResponseMessage>>> sendAsyncHandler)
        {
            httpMessageHandler = new Mock<HttpMessageHandler>();
            sendAsyncHandler(httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()));

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri(BaseApiAddress)
            };

            return new TestPiipanApiService(client);
        }

        #region GetFromApi
        [Fact]
        public async Task GetFromApi_SendsCorrectRequest_WhenRequestObject_IsNull()
        {
            // Arrange
            var piipanApiService = CreateApiService(n => n.ReturnsAsync(new HttpResponseMessage
            {
                Content = JsonContent.Create(DummyResponse)
            }));

            // Act
            var response = await piipanApiService.GetFromApi<int>(ApiPath);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(1, response.Value);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(n =>
                    n.RequestUri!.ToString() == $"{BaseApiAddress}{ApiPath}"
                    && n.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetFromApi_SendsCorrectRequest_WhenRequestObject_IsNotNull()
        {
            // Arrange
            var piipanApiService = CreateApiService(n => n.ReturnsAsync(new HttpResponseMessage
            {
                Content = JsonContent.Create(DummyResponse)
            }));

            // Act
            var response = await piipanApiService.GetFromApi<int>(ApiPath, new { Prop1 = 2, Prop2 = "3" });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(1, response.Value);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(n =>
                    n.RequestUri!.ToString() == $"{BaseApiAddress}{ApiPath}?Prop1=2&Prop2=3"
                    && n.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetFromApi_SendsApiResponseWithError_WhenExceptionIsThrown()
        {
            // Arrange
            var piipanApiService = CreateApiService(n => n.ThrowsAsync(new Exception("Cannot parse JSON")));

            // Act
            var response = await piipanApiService.GetFromApi<int>(ApiPath);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Errors);
            Assert.Contains(new ServerError("", "Error fetching data. Please try again later."), response.Errors);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(n =>
                    n.RequestUri!.ToString() == $"{BaseApiAddress}{ApiPath}"
                    && n.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
        #endregion GetFromApi
        #region PostToApi
        [Fact]
        public async Task PostToApi_SendsCorrectRequest_WhenRequestObject_IsNull()
        {
            // Arrange
            var piipanApiService = CreateApiService(n => n.ReturnsAsync(new HttpResponseMessage
            {
                Content = JsonContent.Create(DummyResponse)
            }));

            // Act
            var response = await piipanApiService.PostToApi<TestRecord, int>(ApiPath, ItemToSave);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(1, response.Value);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    n => n.RequestUri!.ToString() == $"{BaseApiAddress}{ApiPath}"
                    && n.Content.ReadAsAsync<TestRecord>().Result == ItemToSave
                    && n.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostToApi_SendsApiResponseWithError_WhenExceptionIsThrown()
        {
            // Arrange
            var piipanApiService = CreateApiService(n => n.ThrowsAsync(new Exception("Cannot parse JSON")));

            // Act
            var response = await piipanApiService.PostToApi<TestRecord, int>(ApiPath, ItemToSave);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Errors);
            Assert.Contains(new ServerError("", "Error saving data. Please try again later."), response.Errors);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    n => n.RequestUri!.ToString() == $"{BaseApiAddress}{ApiPath}"
                    && n.Content.ReadAsAsync<TestRecord>().Result == ItemToSave
                    && n.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }
        #endregion PostToApi
    }
}
