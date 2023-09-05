using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Components.Enums;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.QueryTool.Controllers;
using Piipan.Shared.Roles;
using Piipan.Shared.Tests.Mocks;
using Piipan.Shared.Web;
using Xunit;

namespace Piipan.QueryTool.Tests.Controllers
{
    public class MatchControllerTests
    {
        private Mock<ILogger<MatchController>> _loggerMock;
        private Mock<IRolesProvider> _rolesProviderMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IWebAppDataServiceProvider> _webAppDataServiceProviderMock;
        private Mock<IMatchResolutionApi> _matchResolutionApiMock;

        private void InitializeNullMocks()
        {
            _rolesProviderMock ??= DefaultMocks.RoleProviderMock();
            _loggerMock ??= DefaultMocks.ILoggerMock<MatchController>();
            _webAppDataServiceProviderMock = DefaultMocks.IWebAppDataServiceProviderMock();
            _matchResolutionApiMock = DefaultMocks.IMatchResolutionApiMock();

            if (_serviceProviderMock == null)
            {
                _serviceProviderMock = new Mock<IServiceProvider>();
                _serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(_rolesProviderMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(ILogger<MatchController>))).Returns(_loggerMock.Object);
                _serviceProviderMock.Setup(c => c.GetService(typeof(IWebAppDataServiceProvider))).Returns(_webAppDataServiceProviderMock.Object);
            }
        }

        private MatchController CreateController()
        {
            MatchController controller = new MatchController(_matchResolutionApiMock.Object, _serviceProviderMock.Object);
            DefaultMocks.HttpContextMock(controller);

            return controller;
        }

        #region GetMatchDetailByID
        [Theory]
        [InlineData("123")] // match ID too short
        [InlineData("12345678")] // match ID too long
        [InlineData("m1$2345")] // match ID contains special characters
        [InlineData("")] // match ID is required
        [InlineData("INVALID")] // match does not exist
        public async Task GetMatchDetailById_InvalidMatchId_ReturnsUnauthorized(string matchId)
        {
            // Arrange
            InitializeNullMocks();
            MatchController controller = CreateController();


            // Act - Invalid match ID
            var apiResponse = await controller.GetMatchDetailById(matchId);

            // Assert
            Assert.True(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetMatchDetailById_InvalidRole_ReturnsUnauthorized()
        {
            // Arrange
            InitializeNullMocks();
            _webAppDataServiceProviderMock.Setup(n => n.Role).Returns("Other");
            MatchController controller = CreateController();

            // Act - Valid match ID, but unauthorized role
            var apiResponse = await controller.GetMatchDetailById(DefaultMocks.ValidMatchIDFromEA);

            // Assert
            Assert.True(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetMatchDetailById_ValidMatchId_ReturnsMatch()
        {
            // Arrange
            InitializeNullMocks();
            MatchController controller = CreateController();

            // Act - Invalid match ID
            var apiResponse = await controller.GetMatchDetailById(DefaultMocks.ValidMatchIDFromEA);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Equal(DefaultMocks.ValidMatchIDFromEA, apiResponse.Value.Data.MatchId);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetMatchDetailById_GetMatch_ThrowsException()
        {
            // Arrange
            InitializeNullMocks();
            _matchResolutionApiMock.Setup(n => n.GetMatch(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("An error"));
            MatchController controller = CreateController();

            // Act - Valid match ID, but unauthorized role
            var apiResponse = await controller.GetMatchDetailById(DefaultMocks.ValidMatchIDFromEA);

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.NotEmpty(apiResponse.Errors);
            Assert.Equal("Internal Server Error. Please contact system maintainers.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(500, controller.Response.StatusCode);
        }
        #endregion GetMatchDetailByID

        #region GetAllMatchDetails
        [Fact]
        public async Task GetAllMatchDetails_InvalidRequest_ReturnsUnauthorized()
        {
            // Arrange
            InitializeNullMocks(); // Location defaulted to EA

            MatchController controller = CreateController();

            // Act - User is not a national user
            var apiResponse = await controller.GetAllMatchDetails();

            // Assert
            Assert.True(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetAllMatchDetails_ValidRequest_ApiThrowsError()
        {
            // Arrange
            InitializeNullMocks();
            _webAppDataServiceProviderMock.Setup(n => n.Location).Returns("National");
            _webAppDataServiceProviderMock.Setup(n => n.States).Returns(new string[] { "*" });
            _matchResolutionApiMock.Setup(n => n.GetMatches()).Throws(new Exception("An Error"));

            MatchController controller = CreateController();

            // Act - Will throw an error.
            var apiResponse = await controller.GetAllMatchDetails();

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.NotEmpty(apiResponse.Errors);
            Assert.Equal("Internal Server Error. Please contact system maintainers.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(500, controller.Response.StatusCode);
        }

        [Fact]
        public async Task GetAllMatchDetails_ValidRequest_ReturnsMatches()
        {
            // Arrange
            InitializeNullMocks();
            _webAppDataServiceProviderMock.Setup(n => n.Location).Returns("National");
            _webAppDataServiceProviderMock.Setup(n => n.States).Returns(new string[] { "*" });

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.GetAllMatchDetails();

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            var matchList = apiResponse.Value.Data.ToList();
            Assert.Equal(2, matchList.Count);

            // Validate unique data for first match
            Assert.Equal(DefaultMocks.ValidMatchIDFromEA, matchList[0].MatchId);
            Assert.Equal("ea", matchList[0].Initiator);

            // Validate unique data for second match
            Assert.Equal(DefaultMocks.ValidMatchIDFromEB, matchList[1].MatchId);
            Assert.Equal("eb", matchList[1].Initiator);

            // Validate common data for all matches
            for (int i = 0; i < matchList.Count; i++)
            {
                Assert.Equal(MatchRecordStatus.Open, matchList[i].Status);
                Assert.Empty(matchList[i].Dispositions);
                Assert.Empty(matchList[i].Participants);
                Assert.Equal(new string[] { "ea", "eb" }, matchList[i].States);
            }
            Assert.Equal(200, controller.Response.StatusCode);
        }
        #endregion GetAllMatchDetails

        #region SaveMatchUpdate
        private DispositionModel DefaultMatchUpdate() => new DispositionModel
        {
            VulnerableIndividual = true
        };

        [Fact]
        public async Task SaveMatchUpdate_InvalidRequest_ReturnsUnauthorized()
        {
            // Arrange
            InitializeNullMocks(); // Save update as a different state
            _webAppDataServiceProviderMock.Setup(n => n.Role).Returns("Oversight"); // only Worker role can edit matches

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.True(apiResponse.IsUnauthorized);
            Assert.Equal("You do not have the role and permissions to edit match details.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ValidRequest_ReturnsNewMatchRecord()
        {
            // Arrange
            InitializeNullMocks();

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Single(apiResponse.Value.Alerts);
            Assert.Equal(AlertSeverity.Success, apiResponse.Value.Alerts[0].AlertSeverity);
            Assert.Equal("<strong>Your update has been successfully saved.</strong>", apiResponse.Value.Alerts[0].Html);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task TestValidateCloseMatchAlert()
        {
            // Arrange
            InitializeNullMocks();
            var originalMatch = new MatchResApiResponse
            {
                Data = new MatchDetailsDto
                {
                    States = new string[] { "ea", "eb" },
                    Initiator = "ea",
                    MatchId = DefaultMocks.ValidMatchIDFromEA
                }
            };
            var postSaveMatch = new MatchResApiResponse
            {
                Data = new MatchDetailsDto
                {
                    States = new string[] { "ea", "eb" },
                    Initiator = "ea",
                    MatchId = DefaultMocks.ValidMatchIDFromEA,
                    Status = MatchRecordStatus.Closed
                }
            };
            _matchResolutionApiMock
                .SetupSequence(n => n.GetMatch(DefaultMocks.ValidMatchIDFromEA, "EA"))
                .ReturnsAsync(originalMatch)
                .ReturnsAsync(postSaveMatch);

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // act
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Equal(2, apiResponse.Value.Alerts.Count);
            Assert.Equal(AlertSeverity.Success, apiResponse.Value.Alerts[0].AlertSeverity);
            Assert.Equal("<strong>Your update has been successfully saved.</strong>", apiResponse.Value.Alerts[0].Html);
            Assert.Equal(AlertSeverity.Success, apiResponse.Value.Alerts[1].AlertSeverity);
            Assert.Equal("<strong>This match has been successfully closed.</strong>", apiResponse.Value.Alerts[1].Html);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ValidRequest_ApiFailsReturningMatch_NoErrors()
        {
            // Arrange
            InitializeNullMocks(); // Save update as a different state
            _matchResolutionApiMock.Setup(n => n.GetMatch(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((MatchResApiResponse)null);

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.True(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ValidRequest_ApiGenericError_ReturnsErrors()
        {
            // Arrange
            InitializeNullMocks(); // Save update as a different state
            _matchResolutionApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>()))
                .ReturnsAsync((null, "Some generic error")); // This causes an exception since it's not JSON

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Equal("There was an error saving your data. Please try again.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(500, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ValidRequest_ApiEmptyFailureJson_ReturnsErrors()
        {
            // Arrange
            InitializeNullMocks(); // Save update as a different state
            _matchResolutionApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>()))
                .ReturnsAsync((null, "{}")); // This can't be translated to an ApiErrorResponse

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Equal("There was an error saving your data. Please try again.", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ValidRequest_ApiReturningDuplicateMatch_AddsAlert()
        {
            // Arrange
            InitializeNullMocks(); // Save update as a different state
            _matchResolutionApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>()))
                .ReturnsAsync((null, "{ \"errors\": [ { \"status\": \"\", \"title\": \"\", \"detail\": \"Duplicate action not allowed\" } ] }"));

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Empty(apiResponse.Errors);
            Assert.NotNull(apiResponse.Value);
            Assert.Single(apiResponse.Value.Alerts);
            Assert.Equal(AlertSeverity.Info, apiResponse.Value.Alerts[0].AlertSeverity);
            Assert.Equal("<strong>There are no changes to save.</strong>", apiResponse.Value.Alerts[0].Html);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ValidRequest_ApiErrorResponse_ReturnsErrors()
        {
            // Arrange
            InitializeNullMocks(); // Save update as a different state
            _matchResolutionApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>()))
                .ReturnsAsync((null, "{ \"errors\": [ { \"status\": \"\", \"title\": \"\", \"detail\": \"API Error 1\" }, { \"status\": \"\", \"title\": \"\", \"detail\": \"API Error 2\" } ] }"));

            MatchController controller = CreateController();

            // Act
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Equal("API Error 1", apiResponse.Errors[0].Error);
            Assert.Equal("API Error 2", apiResponse.Errors[1].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }

        [Fact]
        public async Task SaveMatchUpdate_ModelError_ReturnsServerError()
        {
            // Arrange
            InitializeNullMocks();
            MatchController controller = CreateController();
            controller.ModelState.AddModelError("SomeProperty", "Some Error");

            // Act - Perform invalid search
            var apiResponse = await controller.SaveMatchUpdate(DefaultMocks.ValidMatchIDFromEA, DefaultMatchUpdate());

            // Assert
            Assert.False(apiResponse.IsUnauthorized);
            Assert.Equal("DispositionData.SomeProperty", apiResponse.Errors[0].Property);
            Assert.Equal("Some Error", apiResponse.Errors[0].Error);
            Assert.Null(apiResponse.Value);
            Assert.Equal(200, controller.Response.StatusCode);
        }
        #endregion SaveMatchUpdate
    }
}
