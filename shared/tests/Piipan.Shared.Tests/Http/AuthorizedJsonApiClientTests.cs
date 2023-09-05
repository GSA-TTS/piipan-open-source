using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Piipan.Components.Modals;
using Piipan.Shared.Authentication;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Shared.Tests.Http
{
    public class AuthorizedJsonApiClientTests
    {
        public class FakeRequestType
        {
            public string RequestMessage { get; set; }
        }

        public class FakeResponseType
        {
            public string ResponseMessage { get; set; }
            public int StatusCode { get; set; }
        }

        [Fact]
        public async Task PostAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Post && m.RequestUri.ToString() == "https://tts.test/path"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };

            // Act
            var response = await apiClient.PostAsync<FakeRequestType, FakeResponseType>("/path", body);

            // Assert
            Assert.IsType<FakeResponseType>(response);
            Assert.Equal("this is a response message", response.ResponseMessage);
        }

        [Fact]
        public async Task PostAsync_IncludesAdditionalHeaders()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m =>
                        m.Method == HttpMethod.Post &&
                        m.RequestUri.ToString() == "https://tts.test/path" &&
                        m.Headers.Contains("added-header")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };

            // Act
            var response = await apiClient.PostAsync<FakeRequestType, FakeResponseType>("/path", body, () =>
            {
                return new List<(string, string)>
                {
                    ("added-header", "added-value")
                };
            });

            // Assert
            Assert.IsType<FakeResponseType>(response);
            Assert.Equal("this is a response message", response.ResponseMessage);
        }

        [Fact]
        public async Task PatchAsync_IncludesAdditionalHeaders()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m =>
                        m.Method == HttpMethod.Patch &&
                        m.RequestUri.ToString() == "https://tts.test/path" &&
                        m.Headers.Contains("added-header")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };

            // Act
            var response = await apiClient.PatchAsync<FakeRequestType, FakeResponseType>("/path", body, () =>
            {
                return new List<(string, string)>
                {
                    ("added-header", "added-value")
                };
            });

            // Assert
            Assert.IsType<(FakeResponseType, string)>(response);
            Assert.Equal("this is a response message", response.SuccessResponse.ResponseMessage);
        }

        [Fact]
        public async Task GetAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.ToString() == "https://tts.test/path"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            // Act
            var response = await apiClient.GetAsync<FakeResponseType>("/path");

            // Assert
            Assert.IsType<FakeResponseType>(response);
            Assert.Equal("this is a response message", response.ResponseMessage);
        }

        [Fact]
        public async Task TryGetAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.ToString() == "https://tts.test/path"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            // Act
            var response = await apiClient.TryGetAsync<FakeResponseType>("/path");

            // Assert
            Assert.IsType<FakeResponseType>(response.Response);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("this is a response message", response.Response.ResponseMessage);
        }

        [Fact]
        public async Task TryGetAsync_SendsNullOn404()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound) { };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.ToString() == "https://tts.test/path"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            // Act
            var response = await apiClient.TryGetAsync<FakeResponseType>("/path");

            // Assert
            Assert.Null(response.Response);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TryGetAsync_ThrowsErrorOnOtherStatus()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized) { };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.ToString() == "https://tts.test/path"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );

            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await apiClient.TryGetAsync<FakeResponseType>("/path"));
        }
        private Mock<HttpMessageHandler> CreateHandler(HttpResponseMessage httpResponse, HttpMethod httpMethod)
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
                   mockMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == httpMethod  && m.RequestUri.ToString() == "https://tts.test/path"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse); 
            return mockMessageHandler;
        }

        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [Theory]
        public async Task SendAsyncGet_Retries3Times_AsExpected(HttpStatusCode statusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = CreateHandler(httpResponse, HttpMethod.Get);

            AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> apiClient = GetApiClient(httpMessageHandler);
            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await apiClient.GetAsync<HttpResponseMessage>("/path"));
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(apiClient.NoOfRetries+1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [Theory]
        public async Task SendAsyncTryGet_Retries3Times_AsExpected(HttpStatusCode statusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = CreateHandler(httpResponse, HttpMethod.Get);
            AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> apiClient = GetApiClient(httpMessageHandler);

            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await apiClient.TryGetAsync<HttpResponseMessage>("/path"));
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(apiClient.NoOfRetries+1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [Theory]
        public async Task SendAsyncPost_Retries3Times_AsExpected(HttpStatusCode statusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Post, "https://tts.test/path");
            var httpMessageHandler = CreateHandler(httpResponse, HttpMethod.Post);
            AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> apiClient = GetApiClient(httpMessageHandler);
            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };

          
            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await apiClient.PostAsync<FakeRequestType, FakeResponseType>("/path", body));
 
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(apiClient.NoOfRetries+1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [Theory]
        public async Task SendAsync_Patch_Retries3Times_AsExpected(HttpStatusCode statusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

         
            var httpMessageHandler = CreateHandler(httpResponse, HttpMethod.Patch);
            AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> apiClient = GetApiClient(httpMessageHandler);
            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };


            // Assert
            var response = await apiClient.PatchAsync<FakeRequestType, FakeResponseType>("/path", body, () =>
            {
                return new List<(string, string)>
                {
                    ("added-header", "added-value")
                };
            });

            // Assert
            Assert.Contains(statusCode.ToString(), response.FailResponse);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(apiClient.NoOfRetries+1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
        [InlineData(HttpStatusCode.Conflict)]
        [Theory]
        public async Task SendAsyncGet_No_RetryForCodes_NotInPolicy(HttpStatusCode statusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = CreateHandler(httpResponse, HttpMethod.Get);
            AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> apiClient = GetApiClient(httpMessageHandler);
            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await apiClient.GetAsync<HttpResponseMessage>("/path"));
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
        [InlineData(HttpStatusCode.Conflict)]
        [Theory]
        public async Task SendAsyncPost_No_RetryForCodes_NotInPolicy(HttpStatusCode statusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Post, "https://tts.test/path");
            var httpMessageHandler = CreateHandler(httpResponse, HttpMethod.Post);
            AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> apiClient = GetApiClient(httpMessageHandler);
            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };


            // Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await apiClient.PostAsync<FakeRequestType, FakeResponseType>("/path", body));

            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        private AuthorizedJsonApiClient<AuthorizedJsonApiClientTests> GetApiClient(Mock<HttpMessageHandler> httpMessageHandler)
        {
            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);

            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );
            //Overriding to speed up the 
            apiClient.RetryInterval = 1;
            apiClient.NoOfRetries = 1;
            return apiClient;
        }
    }
}