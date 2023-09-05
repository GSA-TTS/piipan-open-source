using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Shared.Logging.Tests
{
    public class AuthenticationLoggingMiddlewareTests
    {
        ClaimsOptions claimsOptions = new ClaimsOptions
        {
            Email = "email_claim_type",
            Role = "app_role_claim_type",
            LocationPrefix = "Location-",
            RolePrefix = "Role-"
        };

        [Fact]
        public async void InvokeAsync_NewSession()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new AuthenticationLoggingMiddleware(requestDelegate);

            var claims = new List<Claim> 
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3")
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));
            
            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User)
                .Returns(claimsPrincipal);
            
            byte[] val = null;
            var session = new Mock<ISession>();

            session
                .Setup(m => m.Id)
                .Returns("ABCD1234");

            session
                .Setup(m => m.TryGetValue(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, out val))
                .Returns(true);
            
            session
                .Setup(m => m.Set(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k,v) => val = v);
            
            httpContext
                .Setup(m => m.Session)
                .Returns(session.Object);

            var logger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();

            var options = Options.Create(claimsOptions);
            var logger2 = new Mock<ILogger<ClaimsProvider>>();
            var claimsProvider = new Mock<IClaimsProvider>();

            claimsProvider.Setup(m => m.GetEmail(claimsPrincipal)).Returns("email_claim_type");
            
            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object, claimsProvider.Object);

            // Assert
            foreach (var claim in claims)
            {
                logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Session: ABCD1234][CLAIM] {claim.Type}: {claim.Value}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
            }

            
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"User logged in: email_claim_type")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());

            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"User logged out: email_claim_type")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never());

            Assert.NotNull(val);
        }

        [Fact]
        public async void InvokeAsync_NewSessionZeroClaimsLoggedKey()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new AuthenticationLoggingMiddleware(requestDelegate);

            var claims = new List<Claim>
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3")
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));

            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User)
                .Returns(claimsPrincipal);

            byte[] val = BitConverter.GetBytes(0);
            var session = new Mock<ISession>();

            session
                .Setup(m => m.Id)
                .Returns("ABCD1234");

            session
                .Setup(m => m.TryGetValue(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, out val))
                .Returns(true);

            session
                .Setup(m => m.Set(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k, v) => val = v);

            httpContext
                .Setup(m => m.Session)
                .Returns(session.Object);

            var logger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();

            var options = Options.Create(claimsOptions);
            var logger2 = new Mock<ILogger<ClaimsProvider>>();
            var claimsProvider = new Mock<IClaimsProvider>();

            claimsProvider.Setup(m => m.GetEmail(claimsPrincipal)).Returns("email_claim_type");

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object, claimsProvider.Object);

            // Assert
            foreach (var claim in claims)
            {
                logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Session: ABCD1234][CLAIM] {claim.Type}: {claim.Value}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
            }


            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"User logged in: email_claim_type")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());

            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"User logged out: email_claim_type")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never());

            Assert.NotNull(val);
        }

        [Fact]
        public async void InvokeAsync_ReturningSession()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new AuthenticationLoggingMiddleware(requestDelegate);

            var claims = new List<Claim> 
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3")
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));
            
            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User)
                .Returns(claimsPrincipal);

            byte[] val = BitConverter.GetBytes(1);
            var session = new Mock<ISession>();
            session
                .Setup(m => m.TryGetValue(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, out val))
                .Returns(true);
            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options);

            session
                .Setup(m => m.Set(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k,v) => val = v);
            
            httpContext
                .Setup(m => m.Session)
                .Returns(session.Object);

            var logger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object, claimsProvider);

            // Assert
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"[CLAIM]")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never());

            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"User logged out: email_claim_type")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async void InvokeAsync_LogOutHeader()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new AuthenticationLoggingMiddleware(requestDelegate);

            var claims = new List<Claim>
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("email", "value3")
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));

            var request = new Mock<HttpRequest>();

            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "Referer", "logout"}
            }) as IHeaderDictionary;
            request
                .Setup(m => m.Headers)
                .Returns(headers);


            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User)
                .Returns(claimsPrincipal);


            byte[] val = BitConverter.GetBytes(1);
            var session = new Mock<ISession>();
            session
                .Setup(m => m.TryGetValue(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, out val))
                .Returns(true);

            var emailClaimVal = Encoding.ASCII.GetBytes("email_claim_type");

            session
                 .Setup(m => m.TryGetValue("USER_EMAIL", out emailClaimVal))
                 .Returns(true);

            var claimsProvider = new Mock<IClaimsProvider>();
            claimsProvider.Setup(m => m.GetEmail(claimsPrincipal)).Returns("email_claim_type");

            session
                .Setup(m => m.Set(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k, v) => val = v);

            httpContext
                .Setup(m => m.Session)
                .Returns(session.Object);

            var logger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
            //act before sign out header
            await middleware.InvokeAsync(httpContext.Object, logger.Object, claimsProvider.Object);

            httpContext.Setup(m => m.Request).Returns(request.Object);

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object, claimsProvider.Object);

            // Assert
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"User logged out: email_claim_type")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }
    }
}