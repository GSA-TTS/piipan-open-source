using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Metrics.Func.Api.Tests
{
    public class GetUploadStatisticsTests
    {
        [Fact]
        public async Task Run_Success()
        {
            // Arrange
            var uploadedAt = DateTime.Now;

            var context = new DefaultHttpContext();
            var request = context.Request;

            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadReaderApi>();

            var expectedResponse = new ParticipantUploadStatistics
            {
                TotalComplete = 2,
                TotalFailure = 1
            };

            uploadApi
                .Setup(m => m.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()))
                .ReturnsAsync(expectedResponse);

            var function = new GetUploadStatistics(uploadApi.Object);

            // Act
            var result = await function.Run(request, logger.Object) as JsonResult;
            var response = result.Value as ParticipantUploadStatistics;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(response.TotalComplete, expectedResponse.TotalComplete);
            Assert.Equal(response.TotalFailure, expectedResponse.TotalFailure);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing request from user")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task Run_UploadApiThrows()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;

            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadReaderApi>();
            uploadApi
                .Setup(m => m.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new GetUploadStatistics(uploadApi.Object);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => function.Run(request, logger.Object));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}