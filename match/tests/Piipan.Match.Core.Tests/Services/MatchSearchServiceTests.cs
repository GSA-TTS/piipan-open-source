using FluentValidation;
using FluentValidation.Results;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Services;
using Piipan.Participants.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Piipan.Shared.Cryptography;
using Piipan.Shared.API.Enums;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchSearchServiceTests
    {
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private ICryptographyClient cryptographyClient;

        public MatchSearchServiceTests()
        {
            cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);
        }

        [Fact]
        public async Task ReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var requestPersonValidator = Mock.Of<IValidator<RequestPerson>>();
            var service = new MatchSearchService(participantApi, requestPersonValidator,cryptographyClient);

            var request = new OrchMatchRequest();

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Results);
            Assert.Empty(response.Data.Errors);
        }

        [Fact]
        public async Task ReturnsErrorsForInvalidPersons()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("property", "invalid value")
                }));

            var service = new MatchSearchService(participantApi, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Results);
            Assert.Single(response.Data.Errors);
            Assert.Single(response.Data.Errors, e => e.Index == 0);
            Assert.Single(response.Data.Errors, e => e.Detail == "invalid value");
        }

        [Fact]
        public async Task ReturnsResultsForValidPersons()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchSearchService(participantApi, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Errors);
            Assert.Single(response.Data.Results);
            Assert.Single(response.Data.Results, r => r.Index == 0);
        }

        [Theory]
        [InlineData("Application", ValidSearchReasons.application)]
        [InlineData("Recertification", ValidSearchReasons.recertification)]
        [InlineData("New_household_member", ValidSearchReasons.new_household_member)]
        [InlineData("OTHER", ValidSearchReasons.other)]
        public async Task ValidSearchReason(string searchReason, ValidSearchReasons expectedSearchReason)
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchSearchService(participantApi, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = searchReason }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Errors);
            Assert.Single(response.Data.Results);
            Assert.Single(response.Data.Results, r => r.Index == 0);
        }

        [Theory]
        [InlineData("ASDF")]
        [InlineData("New household member")]
        [InlineData("")]
        public async Task InvalidSearchReason(string searchReason)
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("searchReason", "StringEnumValidator")
                }));

            var service = new MatchSearchService(participantApi, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = searchReason }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response.Data.Errors);
            Assert.Equal("StringEnumValidator", response.Data.Errors[0].Detail);
        }

        [Fact]
        public async Task ReturnsAggregatedMatchesFromStates()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea", "eb" });

            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ParticipantMatch>
                {
                    new ParticipantMatch { ParticipantId = "p1" },
                    new ParticipantMatch { ParticipantId = "p2" }
                });

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchSearchService(participantApi.Object, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Errors);
            Assert.Single(response.Data.Results);
            Assert.Single(response.Data.Results, r => r.Index == 0);

            var matches = response.Data.Results.First().Matches;
            Assert.Equal(2, matches.Count());
            Assert.Equal(1, matches.Count(m => m.ParticipantId == "p1"));
            Assert.Equal(1, matches.Count(m => m.ParticipantId == "p2"));
        }

        [Fact]
        public async Task ThrowsWhenRequestPersonValidatorThrows()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("validator failed"));

            var service = new MatchSearchService(participantApi, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => service.FindAllMatches(request, "ea"));
        }

        [Fact]
        public async Task ThrowsWhenParticipantApiThrows()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea", "eb" });
            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("participant API failed"));

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchSearchService(participantApi.Object, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => service.FindAllMatches(request, "ea"));
        }
        [Fact]
        public async Task ReturnsAggregatedMatchesFromSameState()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea" });

            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ParticipantMatch>
                {
                    new ParticipantMatch { ParticipantId = "p1" },
                    new ParticipantMatch { ParticipantId = "p2" }
                });

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchSearchService(participantApi.Object, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Errors);
            Assert.Single(response.Data.Results);
            Assert.Single(response.Data.Results, r => r.Index == 0);

            var matches = response.Data.Results.First().Matches;
            Assert.Empty(matches);
            Assert.Equal(0, matches.Count(m => m.ParticipantId == "p1"));
            Assert.Equal(0, matches.Count(m => m.ParticipantId == "p2"));
        }
        [Fact]
        public async Task ReturnsEmptyMatchesFromSameState()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea" });

            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ParticipantMatch>
                {
                    new ParticipantMatch { ParticipantId = "p1" },
                });

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchSearchService(participantApi.Object, requestPersonValidator.Object, cryptographyClient);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "", SearchReason = "other" }
                }
            };

            // Act
            var response = await service.FindAllMatches(request, "ea");

            participantApi.Verify(r => r.GetParticipants("ea", request.Data[0].LdsHash), Times.Never);

        }
    }
}
