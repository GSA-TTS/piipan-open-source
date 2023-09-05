using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.QueryTool.Controllers;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Deidentification;
using Piipan.Shared.Roles;
using Piipan.Shared.Tests.Mocks;
using Piipan.Shared.Web;
using Xunit;

namespace Piipan.QueryTool.Tests.Controllers
{
    public class DuplicateParticipantSearchControllerTests
    {
        private Mock<ILogger<DuplicateParticipantSearchController>> _loggerMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IWebAppDataServiceProvider> _webAppDataServiceProviderMock;
        private Mock<IMatchSearchApi> _matchApiMock;
        private Mock<ILdsDeidentifier> _ldsDeidentifierMock;
        private IConfiguration _configuration;

        private DuplicateParticipantQuery DefaultQuery() => new()
        {
            LastName = "Farrington",
            SocialSecurityNum = "887-65-4320",
            DateOfBirth = new DateTime(1931, 10, 13),
            ParticipantId = "participantid1",
            SearchReason = "application"
        };
        private void InitializeNullMocks()
        {
            _ldsDeidentifierMock ??= DefaultMocks.ILdsDeidentifierMock();
            _loggerMock ??= DefaultMocks.ILoggerMock<DuplicateParticipantSearchController>();
            var _roleProvider = DefaultMocks.RoleProviderMock();
            _webAppDataServiceProviderMock = DefaultMocks.IWebAppDataServiceProviderMock();
            _matchApiMock = DefaultMocks.IMatchApiMock();

            var inMemorySettings = new Dictionary<string, string> {
                {"AuthorizationPolicy:RequiredClaims:0:Type", "email" },
                {"AuthorizationPolicy:RequiredClaims:0:Values:0", "*"},
            };

            _configuration = DefaultMocks.ConfigurationMock(inMemorySettings);

            if (_serviceProviderMock == null)
            {
                _serviceProviderMock = new Mock<IServiceProvider>();
                _serviceProviderMock.Setup(c => c.GetService(typeof(IConfiguration))).Returns(_configuration);
                _serviceProviderMock.Setup(c => c.GetService(typeof(ILdsDeidentifier))).Returns(_ldsDeidentifierMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IMatchSearchApi))).Returns(_matchApiMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(ILogger<DuplicateParticipantSearchController>))).Returns(_loggerMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IWebAppDataServiceProvider))).Returns(_webAppDataServiceProviderMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(_roleProvider.Object);
            }
        }

        private DuplicateParticipantSearchController CreateController()
        {
            DuplicateParticipantSearchController controller = new DuplicateParticipantSearchController(_ldsDeidentifierMock.Object, _matchApiMock.Object, _serviceProviderMock.Object);
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

        #region PerformSearch
        [Fact]
        public async Task PerformSearch_ValidQuery_ReturnsMatch()
        {
            // Arrange
            InitializeNullMocks();
            var controller = CreateController();
            DuplicateParticipantQuery query = DefaultQuery();

            // Act - Perform valid search
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.NotEmpty(apiResponse.Value.Results[0].Matches);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task PerformSearch_ValidQuery_ReturnsNoResults()
        {
            // Arrange
            InitializeNullMocks();
            _matchApiMock.Setup(m => m.FindAllMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(new OrchMatchResponse
                {
                    Data = new OrchMatchResponseData
                    {
                        Results = new List<OrchMatchResult>
                        {
                            new OrchMatchResult
                            {
                                Index = 0,
                                Matches = new List<ParticipantMatch>()
                            }
                        },
                        Errors = new List<OrchMatchError>()
                    }
                });
            var controller = CreateController();

            DuplicateParticipantQuery query = DefaultQuery();

            // Act - Perform valid search, but no results
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Empty(apiResponse.Value.Results[0].Matches);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task PerformSearch_ModelError_ReturnsServerError()
        {
            // Arrange
            InitializeNullMocks();
            var controller = CreateController();
            DuplicateParticipantQuery query = DefaultQuery();
            controller.ModelState.AddModelError("SomeProperty", "Some Error");

            // Act - Perform invalid search
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            AssertErrors(apiResponse, "QueryFormData.SomeProperty", "Some Error");
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task PerformSearch_ApiException_ReturnsServerError()
        {
            // Arrange
            InitializeNullMocks();
            _matchApiMock.Setup(m => m.FindAllMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("api broke"));
            var controller = CreateController();
            DuplicateParticipantQuery query = DefaultQuery();

            // Act - Perform invalid search
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            AssertErrors(apiResponse, "", "There was an error running your search. Please try again.");
            Assert.Equal(500, controller.Response.StatusCode);
        }

        [Fact]
        public async Task PerformSearch_WrongLocation_ReturnsError()
        {
            // Arrange
            InitializeNullMocks();
            _webAppDataServiceProviderMock.Setup(m => m.Location)
                .Returns("National");
            var controller = CreateController();
            DuplicateParticipantQuery query = DefaultQuery();

            // Act - Perform invalid search
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            AssertErrors(apiResponse, "", "Search performed with an invalid location");
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task PerformSearch_MultipleStates_ReturnsError()
        {
            // Arrange
            InitializeNullMocks();
            _webAppDataServiceProviderMock.Setup(m => m.States)
                .Returns(new string[] { "EA", "EB" });
            var controller = CreateController();
            DuplicateParticipantQuery query = DefaultQuery();

            // Act - Perform invalid search
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            AssertErrors(apiResponse, "", "Search performed with an invalid location");
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Theory]
        [InlineData("something gregorian something", "Date of birth must be a real date.")]
        [InlineData("something something something", "something something something")]
        public async Task PerformSearch_LdsHashException_ReturnsServerError(string exceptionMessage, string expectedErrorMessage)
        {
            // Arrange
            InitializeNullMocks();
            _ldsDeidentifierMock
                .Setup(m => m.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(exceptionMessage));
            var controller = CreateController();
            DuplicateParticipantQuery query = DefaultQuery();

            // Act - Perform invalid search
            var apiResponse = await controller.PerformSearch(query);

            // Assert
            AssertErrors(apiResponse, "", expectedErrorMessage);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        #endregion PerformSearch
    }
}
