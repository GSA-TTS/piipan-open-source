using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Match.Client.Tests
{
    public class MatchResolutionClientTests
    {
        [Fact]
        public async Task GetMatch_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new MatchResApiResponse
            {
                Data = new MatchDetailsDto
                {
                    Dispositions = new Disposition[] { Mock.Of<Disposition>() },
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    MatchId = "m123456",
                    Participants = new Participant[] { Mock.Of<Participant>() },
                    Status = MatchRecordStatus.Open
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<MatchResolutionClient>>();
            apiClient
                .Setup(m => m.TryGetAsync<MatchResApiResponse>("matches/m123456", It.IsAny<IEnumerable<(string, string)>>(), null))
                .ReturnsAsync((expectedResponse, 200));

            var client = new MatchResolutionClient(apiClient.Object);

            // Act
            var response = await client.GetMatch("m123456", "ea");

            // Assert
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task GetMatches_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new MatchResListApiResponse
            {
                Data = Enumerable.Range(0, 2).Select(index =>
                    new MatchDetailsDto
                    {
                        Dispositions = new Disposition[] { Mock.Of<Disposition>() },
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        MatchId = new string(index.ToString()[0], 7),
                        Participants = new Participant[] { Mock.Of<Participant>() },
                        Status = MatchRecordStatus.Open
                    }
                )
            };

            var apiClient = new Mock<IAuthorizedApiClient<MatchResolutionClient>>();
            apiClient
                .Setup(m => m.TryGetAsync<MatchResListApiResponse>("matches", null, null))
                .ReturnsAsync((expectedResponse, 200));

            var client = new MatchResolutionClient(apiClient.Object);

            // Act
            var response = await client.GetMatches();

            // Assert
            Assert.Equal(expectedResponse, response);
        }
    }
}
