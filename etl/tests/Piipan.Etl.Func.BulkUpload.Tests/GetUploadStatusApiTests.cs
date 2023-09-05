using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class GetUploadStatusApiTests
    {
        [Fact]
        public async void Run_ReturnsUploadDetailsWhenFound()
        {
            // Arrange
            var participantUploadApi = new Mock<IParticipantUploadApi>();
            var logger = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var request = context.Request;

            UploadDto uploadDto = new UploadDto();

            participantUploadApi
                .Setup(m => m.GetUploadById("upload1"))
                .ReturnsAsync(uploadDto);

            var function = new GetUploadStatusApi(participantUploadApi.Object);

            // Act
            var result = await function.GetUploadById(request, "upload1", logger.Object) as JsonResult;
            var response = result.Value as UploadStatusApiResponse;

            // Assert
            Assert.Same(uploadDto, response.Data);
        }

        [Fact]
        public async void GetUploadByIdReturnsCorrectVariables()
        {
            // Arrange
            var participantUploadApi = new Mock<IParticipantUploadApi>();
            var logger = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var request = context.Request;

            UploadDto uploadDto = new UploadDto{
                Id = 12345,
                Publisher = "testPublisher",
                UploadIdentifier = "0x8DA814242535F4D",
                CreatedAt = new DateTime(),
                ParticipantsUploaded = 40,
                CompletedAt = new DateTime(),
                Status =  "COMPLETE"
            };

            

            participantUploadApi
                .Setup(m => m.GetUploadById("upload1"))
                .ReturnsAsync(uploadDto);

            var function = new GetUploadStatusApi(participantUploadApi.Object);

            // Act
            var result = await function.GetUploadById(request, "upload1", logger.Object) as JsonResult;
            var response = result.Value as UploadStatusApiResponse;

            string serializedResponse = JsonConvert.SerializeObject(response);

            // Assert
            Assert.Same(uploadDto, response.Data);
            Assert.DoesNotContain("testPublisher", serializedResponse);
            Assert.DoesNotContain("12345", serializedResponse);
            Assert.Contains("upload_identifier", serializedResponse);
            Assert.Contains("created_at", serializedResponse);
            Assert.Contains("participants_uploaded", serializedResponse);
            Assert.Contains("completed_at", serializedResponse);
            Assert.Contains("status", serializedResponse);
        }


        [Fact]
        public async void GetUploadById_Returns_ApiErrorResponse_ForGeneralErrors()
        {
            // Arrange
            var participantUploadApi = new Mock<IParticipantUploadApi>();
            var logger = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var request = context.Request;

            participantUploadApi
                .Setup(m => m.GetUploadById("upload1"))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new GetUploadStatusApi(participantUploadApi.Object);

            // Act / Assert
            var result = await function.GetUploadById(request, "upload1", logger.Object) as JsonResult;
            Assert.IsType<ApiErrorResponse>(result.Value);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.InternalServerError);

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }

        [Fact]
        public async void GetUploadById_Returns_NotFound_When_UploadId_Does_Not_Exist()
        {
            // Arrange
            var participantUploadApi = new Mock<IParticipantUploadApi>();
            var logger = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var request = context.Request;

            participantUploadApi
                .Setup(m => m.GetUploadById("upload1"))
                .ThrowsAsync(new InvalidOperationException("upload not found"));

            var function = new GetUploadStatusApi(participantUploadApi.Object);

            // Act / Assert
            var result = await function.GetUploadById(request, "upload1", logger.Object) as NotFoundObjectResult;
            Assert.IsType<ApiErrorResponse>(result.Value);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NotFound);

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload not found"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }

        [Fact]
        public async void GetUploadById_Returns_BadRequest_When_Invalid_Space()
        {
            // Arrange
            var participantUploadApi = new Mock<IParticipantUploadApi>();
            var logger = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var request = context.Request;

            var function = new GetUploadStatusApi(participantUploadApi.Object);

            // Act / Assert
            var result = await function.GetUploadById(request, "uploa d1", logger.Object) as BadRequestObjectResult;
            Assert.IsType<ApiErrorResponse>(result.Value);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
            ApiErrorResponse apiResult = result.Value as ApiErrorResponse;
            Assert.Equal("Request was not valid: Id cannot contain a space.", apiResult.Errors[0].Detail);
             
        }

        [Fact]
        public async void GetUploadById_Returns_BadRequest_When_Invalid_Long()
        {
            // Arrange
            var participantUploadApi = new Mock<IParticipantUploadApi>();
            var logger = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var request = context.Request;

            var function = new GetUploadStatusApi(participantUploadApi.Object);

            // Act / Assert
            var result = await function.GetUploadById(request, "uploadTooLong1234567890123456789012345678901234567890", logger.Object) as BadRequestObjectResult;
            Assert.IsType<ApiErrorResponse>(result.Value);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
            ApiErrorResponse apiResult = result.Value as ApiErrorResponse;
            Assert.Equal("Request was not valid: Id cannot exceed 40 characters.", apiResult.Errors[0].Detail);

        }
    }
}
