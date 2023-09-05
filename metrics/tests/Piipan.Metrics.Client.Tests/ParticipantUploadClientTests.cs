using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Metrics.Client.Tests
{
    public class ParticipantUploadClientTests
    {
        [Fact]
        public async Task GetUploads_ReturnsApiClientResponse()
        {
            // Arrange
            ParticipantUploadRequestFilter filter = new ParticipantUploadRequestFilter()
            {
                State = "ea",
                Page = 1,
                PerPage = 20
            };
            var expectedResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "ea", UploadedAt = DateTime.Now }
                },
                Meta = new Meta
                {
                    Total = 1,
                    PageQueryParams = "?PerPage=20&State=ea"
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<ParticipantUploadClient>>();
            apiClient
                .Setup(m => m.GetAsync<GetParticipantUploadsResponse, ParticipantUploadRequestFilter>("GetParticipantUploads", filter))
                .ReturnsAsync(expectedResponse);

            var client = new ParticipantUploadClient(apiClient.Object);

            // Act
            var response = await client.GetUploads(filter);

            // Assert
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task GetLatestUploadsByState_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "ea", UploadedAt = DateTime.Now }
                },
                Meta = new Meta
                {
                    Total = 1
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<ParticipantUploadClient>>();
            apiClient
                .Setup(m => m.GetAsync<GetParticipantUploadsResponse>("GetLastUpload", null))
                .ReturnsAsync(expectedResponse);

            var client = new ParticipantUploadClient(apiClient.Object);

            // Act
            var response = await client.GetLatestUploadsByState();

            // Assert
            Assert.Equal(expectedResponse, response);
        }
    }
}
