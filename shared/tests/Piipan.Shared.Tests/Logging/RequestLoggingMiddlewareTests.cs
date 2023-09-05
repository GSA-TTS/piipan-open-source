using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Shared.Logging.Tests
{
    public class RequestLoggingMiddlewareTests
    {
        [Fact]
        public async void InvokeAsync()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new RequestLoggingMiddleware(requestDelegate);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest
                .Setup(m => m.Method)
                .Returns("GET");
            httpRequest
                .Setup(m => m.Path)
                .Returns(new PathString("/path"));
            
            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.Request)
                .Returns(httpRequest.Object);

            var claimsProvider = new Mock<IClaimsProvider>();
            claimsProvider
                .Setup(m => m.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns("noreply@tts.test");

            var logger = new Mock<ILogger<RequestLoggingMiddleware>>();

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object, claimsProvider.Object);
            
            // Assert
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("noreply@tts.test GET /path")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }
    }
}