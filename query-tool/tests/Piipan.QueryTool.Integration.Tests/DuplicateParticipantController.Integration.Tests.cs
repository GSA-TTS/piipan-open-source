using System.Net.Http.Json;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.API.Validation;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Tests.TestFixtures;
using Xunit;

namespace Piipan.QueryTool.Integration.Tests
{
    public class DuplicateParticipantControllerIntegrationTests
    {
        private DuplicateParticipantQuery DefaultQuery() => new()
        {
            LastName = "Farrington",
            SocialSecurityNum = "123-12-1234",
            DateOfBirth = new DateTime(1931, 10, 13),
            ParticipantId = "participantid1",
            SearchReason = "application"
        };
        private void AssertErrors(ApiResponse<OrchMatchResponseData> apiResponse, string propertyName, string errors)
        {
            Assert.False(apiResponse.IsUnauthorized);
            Assert.NotNull(apiResponse.Errors);
            Assert.Equal(new List<ServerError> {
                            new ServerError(propertyName, errors)
            }, apiResponse.Errors);
            Assert.Null(apiResponse.Value);
        }


        [Theory]
        [InlineData("", ValidationConstants.RequiredMessage)]
        [InlineData("123-12-123A", ValidationConstants.SSNInvalidFormatMessage)]
        [InlineData("000-12-1234", ValidationConstants.SSNInvalidFirstThreeDigitsMessage, "000")]
        [InlineData("123-00-1234", ValidationConstants.SSNInvalidMiddleTwoDigitsMessage)]
        [InlineData("123-12-0000", ValidationConstants.SSNInvalidLastFourDigitsMessage)]
        [InlineData("000-00-0000", $"{ValidationConstants.SSNInvalidFirstThreeDigitsMessage}\n{ValidationConstants.SSNInvalidMiddleTwoDigitsMessage}\n{ValidationConstants.SSNInvalidLastFourDigitsMessage}", "000")]
        public async Task PerformSearch_SSNError_ReturnsServerErrors(string ssn, string errorMessage, params string[] errorFormat)
        {
            // Arrange
            DuplicateParticipantQuery query = DefaultQuery();
            query.SocialSecurityNum = ssn;

            using var piipanServer = new PiipanTestServer<Startup>(this, "EA");

            // Act - Perform invalid search
            var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrchMatchResponseData>>();

            // Assert
            AssertErrors(apiResponse, "QueryFormData.SocialSecurityNum",
                string.Format(errorMessage, errorFormat));
        }

        [Theory]
        [InlineData("", ValidationConstants.RequiredMessage)]
        [InlineData("ABC&*(", ValidationConstants.CanOnlyContainAlphanumericUnderscoreHyphenMessage)]
        [InlineData("12345678901234567890AAA", ValidationConstants.MaxLengthMessage, "20")]
        public async Task PerformSearch_ParticipantIdError_ReturnsServerErrors(string participantId, string errorMessage, params string[] errorFormat)
        {
            // Arrange
            DuplicateParticipantQuery query = DefaultQuery();
            query.ParticipantId = participantId;

            using var piipanServer = new PiipanTestServer<Startup>(this, "EA");

            // Act - Perform invalid search
            var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrchMatchResponseData>>();

            // Assert
            AssertErrors(apiResponse, "QueryFormData.ParticipantId",
                string.Format(errorMessage, errorFormat));
        }

        [Theory]
        [InlineData("ABC&*(", ValidationConstants.CanOnlyContainAlphanumericUnderscoreHyphenMessage)]
        [InlineData("12345678901234567890AAA", ValidationConstants.MaxLengthMessage, "20")]
        public async Task PerformSearch_CaseIdError_ReturnsServerErrors(string caseId, string errorMessage, params string[] errorFormat)
        {
            // Arrange
            DuplicateParticipantQuery query = DefaultQuery();
            query.CaseId = caseId;

            using var piipanServer = new PiipanTestServer<Startup>(this, "EA");

            // Act - Perform invalid search
            var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrchMatchResponseData>>();

            // Assert
            AssertErrors(apiResponse, "QueryFormData.CaseId",
                string.Format(errorMessage, errorFormat));
        }

        [Fact]
        public async Task PerformSearch_LocationError_ReturnsServerErrors()
        {
            // Arrange
            DuplicateParticipantQuery query = DefaultQuery();

            using var piipanServer = new PiipanTestServer<Startup>(this, "National"); // National user cannot perform search

            // Act - Perform invalid search
            var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrchMatchResponseData>>();

            // Assert
            AssertErrors(apiResponse, "",
                "Search performed with an invalid location");
        }

        [Theory]
        [InlineData("", ValidationConstants.RequiredMessage)]
        [InlineData("García", ValidationConstants.InvalidCharacterInNameMessage, "í", "García")]
        [InlineData("-&123", ValidationConstants.MustStartWithALetter)]
        public async Task PerformSearch_NameErrors_ReturnsServerErrors(string name, string errorMessage, params string[] errorFormat)
        {
            // Arrange
            DuplicateParticipantQuery query = DefaultQuery();
            query.LastName = name;

            using var piipanServer = new PiipanTestServer<Startup>(this, "EA");

            // Act - Perform invalid search
            var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrchMatchResponseData>>();

            // Assert
            AssertErrors(apiResponse, "QueryFormData.LastName",
                string.Format(errorMessage, errorFormat));
        }

        [Theory]
        [InlineData("", ValidationConstants.RequiredMessage)]
        [InlineData("12/31/1899", $"{ValidationConstants.ValidationFieldPlaceholder} must be between 01-01-1900 and today's date")]
        [InlineData("12/31/3000", $"{ValidationConstants.ValidationFieldPlaceholder} must be between 01-01-1900 and today's date")]
        public async Task PerformSearch_DateOfBirthErrors_ReturnsServerErrors(string dob, string errorMessage)
        {
            // Arrange
            DuplicateParticipantQuery query = DefaultQuery();
            query.DateOfBirth = string.IsNullOrEmpty(dob) ? null : DateTime.Parse(dob);

            using var piipanServer = new PiipanTestServer<Startup>(this, "EA");

            // Act - Perform invalid search
            var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrchMatchResponseData>>();

            // Assert
            AssertErrors(apiResponse, "QueryFormData.DateOfBirth",
                errorMessage);
        }
    }
}