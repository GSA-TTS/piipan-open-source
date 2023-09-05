using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Metrics.Func.Api.Tests
{
    public class GetLastUploadTests
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

            var expectedResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload
                    {
                        State = "ea",
                        UploadedAt = uploadedAt
                    }
                },
                Meta = new Meta
                {
                    PerPage = 1,
                    Total = 1
                }
            };

            uploadApi
                .Setup(m => m.GetLatestUploadsByState())
                .ReturnsAsync(expectedResponse);

            var function = new GetLastUpload(uploadApi.Object);

            // Act
            var result = await function.Run(request, logger.Object) as JsonResult;
            var response = result.Value as GetParticipantUploadsResponse;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(response.Meta, expectedResponse.Meta);
            Assert.Equal(response.Data, expectedResponse.Data);
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
                .Setup(m => m.GetLatestUploadsByState())
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new GetLastUpload(uploadApi.Object);

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