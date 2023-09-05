using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Dashboard.Controllers;
using Piipan.Match.Api.Models;
using Piipan.Metrics.Api;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Roles;
using Piipan.Shared.Tests.Mocks;
using Piipan.Shared.Web;
using Xunit;

namespace Piipan.Dashboard.Tests.Controllers
{
    public class UploadsControllerTests
    {
        private Mock<ILogger<UploadsController>> _loggerMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IWebAppDataServiceProvider> _webAppDataServiceProviderMock;
        private Mock<IRolesProvider> _rolesProviderMock;
        private Mock<IParticipantUploadReaderApi> _participantUploadReaderApiMock;
        private IConfiguration _configuration;

        private void InitializeNullMocks()
        {
            _loggerMock ??= DefaultMocks.ILoggerMock<UploadsController>();
            _webAppDataServiceProviderMock = DefaultMocks.IWebAppDataServiceProviderMock();
            _rolesProviderMock = DefaultMocks.RoleProviderMock();
            _participantUploadReaderApiMock = DefaultMocks.IParticipantUploadReaderApiMock();

            var inMemorySettings = new Dictionary<string, string> {
                {"AuthorizationPolicy:RequiredClaims:0:Type", "email" },
                {"AuthorizationPolicy:RequiredClaims:0:Values:0", "*"},
            };

            _configuration = DefaultMocks.ConfigurationMock(inMemorySettings);

            if (_serviceProviderMock == null)
            {
                _serviceProviderMock = new Mock<IServiceProvider>();
                _serviceProviderMock.Setup(c => c.GetService(typeof(IConfiguration))).Returns(_configuration);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IParticipantUploadReaderApi))).Returns(_participantUploadReaderApiMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(_rolesProviderMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(ILogger<UploadsController>))).Returns(_loggerMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IWebAppDataServiceProvider))).Returns(_webAppDataServiceProviderMock.Object);
            }
        }

        private UploadsController CreateController()
        {
            UploadsController controller = new UploadsController(_participantUploadReaderApiMock.Object, _serviceProviderMock.Object);
            DefaultMocks.HttpContextMock(controller);

            return controller;
        }

        private void AssertErrors(ApiResponse<OrchMatchResponseData> apiResponse, string propertyName, string errors)
        {
            Assert.False(apiResponse.IsUnauthorized);
            Assert.NotNull(apiResponse.Errors);
            Assert.Equal(new List<ServerError> {
                            new ServerError(propertyName, errors)
            }, apiResponse.Errors);
            Assert.Null(apiResponse.Value);
        }

        #region GetUploads
        private ParticipantUploadRequestFilter DefaultParticipantUploadRequestFilter() =>
            new ParticipantUploadRequestFilter
            {
                State = "EA"
            };

        [Fact]
        public async Task GetUploads_ValidQuery_ReturnsMatch()
        {
            // Arrange
            InitializeNullMocks();
            var controller = CreateController();
            var request = DefaultParticipantUploadRequestFilter();

            // Act - Perform valid search
            var apiResponse = await controller.GetUploads(request);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Equal(1, apiResponse.Value.Total); // By default mocking, total marked as 10
            Assert.Single(apiResponse.Value.ParticipantUploadResults);
            Assert.Equal(10, apiResponse.Value.ParticipantUploadResults[0].ParticipantsUploaded);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetUploads_HttpRequestException_ReturnsAppropriateErrors()
        {
            // Arrange
            InitializeNullMocks();
            _participantUploadReaderApiMock.Setup(n => n.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()))
                .ThrowsAsync(new HttpRequestException("An exception occurred during http request"));
            var controller = CreateController();
            var request = DefaultParticipantUploadRequestFilter();

            // Act - Perform valid search
            var apiResponse = await controller.GetUploads(request);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "An exception occurred during http request"),
                It.IsAny<HttpRequestException>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            Assert.Single(apiResponse.Errors);
            Assert.Equal("", apiResponse.Errors[0].Property);
            Assert.Equal("There was an error loading data. You may be able to try again. If the problem persists, please contact system maintainers.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(500, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetUploads_OtherException_ReturnsAppropriateErrors()
        {
            // Arrange
            InitializeNullMocks();
            _participantUploadReaderApiMock.Setup(n => n.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()))
                .ThrowsAsync(new Exception("An exception occurred"));
            var controller = CreateController();
            var request = DefaultParticipantUploadRequestFilter();

            // Act - Perform valid search
            var apiResponse = await controller.GetUploads(request);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "An exception occurred"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            Assert.Single(apiResponse.Errors);
            Assert.Equal("", apiResponse.Errors[0].Property);
            Assert.Equal("Internal Server Error. Please contact system maintainers.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(500, controller.Response.StatusCode);
        }

        #endregion GetUploads

        #region GetUploadStatistics
        private ParticipantUploadStatisticsRequest DefaultUploadStatisticsRequest() =>
            new ParticipantUploadStatisticsRequest
            {
                HoursOffset = 0
            };

        [Fact]
        public async Task GetUploadStatistics_ValidQuery_ReturnsMatch()
        {
            // Arrange
            InitializeNullMocks();
            var controller = CreateController();
            var request = DefaultUploadStatisticsRequest();

            // Act - Perform valid search
            var apiResponse = await controller.GetUploadStatistics(request);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Equal(1, apiResponse.Value.TotalFailure); // By default mocking, 1 is marked failure
            Assert.Equal(2, apiResponse.Value.TotalComplete); // By default mocking, 2 are marked complete
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetUploadStatistics_Exception_ReturnsAppropriateErrors()
        {
            // Arrange
            InitializeNullMocks();
            _participantUploadReaderApiMock.Setup(n => n.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()))
                .ThrowsAsync(new Exception("An exception occurred"));
            var controller = CreateController();
            var request = DefaultUploadStatisticsRequest();

            // Act - Perform valid search
            var apiResponse = await controller.GetUploadStatistics(request);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "An exception occurred"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            Assert.Single(apiResponse.Errors);
            Assert.Equal("", apiResponse.Errors[0].Property);
            Assert.Equal("Internal Server Error. Please contact system maintainers.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(500, controller.Response.StatusCode);
        }
        #endregion GetUploadStatistics
    }
}
