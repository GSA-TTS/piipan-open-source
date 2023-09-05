using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Cryptography;
using Xunit;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantServiceTests
    {
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private ICryptographyClient cryptographyClient;

        public ParticipantServiceTests()
        {
            cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);
        }

        private IEnumerable<ParticipantDbo> RandomParticipants(int n)
        {
            var result = new List<ParticipantDbo>();

            for (int i = 0; i < n; i++)
            {
                result.Add(new ParticipantDbo
                {
                    LdsHash = cryptographyClient.EncryptToBase64String(Guid.NewGuid().ToString()),
                    CaseId = cryptographyClient.EncryptToBase64String(Guid.NewGuid().ToString()),
                    ParticipantId = cryptographyClient.EncryptToBase64String(Guid.NewGuid().ToString()),
                    ParticipantClosingDate = DateTime.UtcNow.Date,
                    RecentBenefitIssuanceDates = new List<DateRange>(),
                    VulnerableIndividual = (new Random()).Next(2) == 0,
                    UploadId = (new Random()).Next()
                });
            }

            return result;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetParticipants_AllMatchesReturned(int nMatches)
        {
            // Arrange
            var randomLdsHash = Guid.NewGuid().ToString();
            var randomState = Guid.NewGuid().ToString().Substring(0, 2);

            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nMatches);
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(randomState, randomLdsHash, It.IsAny<Int64>()))
                .ReturnsAsync(participants);

            var uploadService = new Mock<IParticipantUploadApi>();
            uploadService
                .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
                .ReturnsAsync(new UploadDto
                {
                    Id = 1,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object,
                uploadService.Object,
                stateService,
                logger,
                cryptographyClient);

            // Act
            var result = await service.GetParticipants(randomState, randomLdsHash);

            // Assert
            // results should have the State set
            var expected = participants.Select(p => new ParticipantDto(p)
            {
                State = randomState,
                ParticipantId = cryptographyClient.DecryptFromBase64String(p.ParticipantId),
                CaseId = cryptographyClient.DecryptFromBase64String(p.CaseId),
            });
            Assert.Equal(expected, result);
        }

        [Fact]
        public async void GetParticipants_UsesLatestUploadId()
        {
            // Arrange
            var randomLdsHash = Guid.NewGuid().ToString();
            var randomState = Guid.NewGuid().ToString().Substring(0, 2);

            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(randomState, randomLdsHash, It.IsAny<Int64>()))
                .ReturnsAsync(new List<ParticipantDbo>());

            var uploadId = (new Random()).Next();
            var uploadService = new Mock<IParticipantUploadApi>();
            uploadService
                .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
                .ReturnsAsync(new UploadDto
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object,
                uploadService.Object,
                stateService,
                logger,
                cryptographyClient);

            // Act
            var result = await service.GetParticipants(randomState, randomLdsHash);

            // Assert
            uploadService.Verify(m => m.GetLatestUpload(It.IsAny<string>()), Times.Once);
            participantDao.Verify(m => m.GetParticipants(randomState, randomLdsHash, uploadId), Times.Once);
        }

        [Fact]
        public async Task GetParticipants_ReturnsEmptyWhenNoUploads()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Int64>()))
                .ReturnsAsync(new List<ParticipantDbo>());

            var uploadId = (new Random()).Next();
            var uploadService = new Mock<IParticipantUploadApi>();
            uploadService
                .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object,
                uploadService.Object,
                stateService,
                logger,
                cryptographyClient);

            // Act
            var participants = await service.GetParticipants("ea", "hash");

            // Assert
            Assert.Empty(participants);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void AddParticipants_AllAddedWithUploadId(int nParticipants)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nParticipants);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            var uploadService = new Mock<IParticipantUploadApi>();

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object,
                uploadService.Object,
                stateService,
                logger,
                cryptographyClient);

            var upload = new UploadDto() { UploadIdentifier="test-etag", Id= uploadId };

            // Act
            await service.AddParticipants(participants, upload, "ea", null);

            // Assert

            // we should verify the status of the upload was set to Complete upon finishing
            uploadService.Verify(m => m.UpdateUpload(It.Is<IUpload>(x => x.Status == UploadStatuses.COMPLETE.ToString()), It.IsAny<string>(), It.Is<string>(x=>x == null)), Times.Once);

            // each participant added via the DAO should have the created upload ID
            participantDao
                .Verify(m => m
                    .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                        participants.All(p => p.UploadId == uploadId)
                    ))
                );

        }

        /// <summary>
        /// When Add Participants has an error, the error is logged with the value of the redaction service.
        /// </summary>
        [Fact]
        public async Task AddParticipants_ThrowsExceptionWhenFailed()
        {
            // Arrange
            var logger = new Mock<ILogger<ParticipantService>>();
            var participants = RandomParticipants(10);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            const string exceptionMessage = "Unhandled error!";
            var thrownException = new Exception(exceptionMessage);
            participantDao.Setup(n => n.AddParticipants(It.IsAny<IEnumerable<ParticipantDbo>>()))
                                .ThrowsAsync(thrownException);
            var foundException = new Exception(exceptionMessage);

            var uploadService = new Mock<IParticipantUploadApi>();
            UploadDto upload = new UploadDto
            {
                Id = uploadId,
                UploadIdentifier = "test-etag",
                CreatedAt = DateTime.UtcNow,
                Publisher = "me",
                Status = UploadStatuses.UPLOADING.ToString()
            };

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object,
                uploadService.Object,
                stateService,
                logger.Object,
                cryptographyClient
                );

            // Act
            await service.AddParticipants(participants, upload, "ea", (ex) => { return ex.Message; });

            // Assert
            Assert.Equal(thrownException.Message, foundException.Message);

            uploadService.Verify(x => x.UpdateUpload(It.IsAny<IUpload>(), It.IsAny<string>(), It.Is<string>(x=>x == null)), Times.Never);
            uploadService.Verify(x => x.UpdateUpload(upload, "ea", exceptionMessage));

        }

        [Fact]
        public async void GetStates_ReturnsDaoResult()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = Mock.Of<IParticipantDao>();
            var uploadService = Mock.Of<IParticipantUploadApi>();

            var stateService = new Mock<IStateService>();
            stateService
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea", "eb", "ec" });

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
            participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                    It.IsAny<ParticipantUpload>()))
                   .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao,
                uploadService,
                stateService.Object,
                logger,
                cryptographyClient);

            // Act
            var result = await service.GetStates();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Collection(result,
                s => Assert.Equal("ea", s),
                s => Assert.Equal("eb", s),
                s => Assert.Equal("ec", s));

        }
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void DeleteOldParticipants_AddedCoupleOfUploads(int nParticipants)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nParticipants);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            var uploadService = new Mock<IParticipantUploadApi>();

            var stateService = Mock.Of<IStateService>();

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
            participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                    It.IsAny<ParticipantUpload>()))
                   .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadService.Object,
                stateService,
                logger,
                cryptographyClient);

            var upload = new UploadDto() { UploadIdentifier = "test-etag", Id = uploadId };

            // Act
            await service.AddParticipants(participants, upload, "ea", null);

            // Now Add another Upload 
            var uploadIdNew = (new Random()).Next();
            var uploadServiceNew = new Mock<IParticipantUploadApi>();

            var serviceNew = new ParticipantService(
               participantDao.Object,
               uploadServiceNew.Object,
               stateService,
               logger,
                cryptographyClient);

            var newUpload = new UploadDto() { UploadIdentifier = "test-etag1", Id = uploadIdNew };

            await serviceNew.AddParticipants(participants, newUpload, "ea", null);

            // Assert

            var uploadServiceDelete = new Mock<IParticipantUploadApi>();
            uploadServiceDelete
               .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
               .ReturnsAsync(new UploadDto
               {
                   Id = uploadIdNew,
                   CreatedAt = DateTime.UtcNow,
                   Publisher = "me",
               });

            var serviceDelete = new ParticipantService(
             participantDao.Object,
             uploadServiceDelete.Object,
             stateService,
             logger,
             cryptographyClient);

            await serviceDelete.DeleteOldParticpants();

            // each participant added via the DAO should have the created upload ID
            participantDao
                .Verify(m => m
                    .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                        participants.All(p => p.UploadId == uploadIdNew)
                    ))
                );
            participantDao
               .Verify(m => m
                   .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                       participants.All(p => p.UploadId != uploadId)
                   ))
               );
        }

    }
}
