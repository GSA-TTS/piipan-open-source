using System;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Api.Models;
using Xunit;

namespace Piipan.Participants.Core.Tests.Models
{
    public class UploadDtoTests
    {
        UploadDto _lhs;

        public UploadDtoTests()
        {
            _lhs = new UploadDto
            {
                Id = 1,
                UploadIdentifier = "abc",
                CreatedAt = DateTime.UtcNow,
                Publisher = "Me",
                ParticipantsUploaded = 50,
                ErrorMessage = "error message",
                CompletedAt = DateTime.UtcNow.AddMinutes(1),
                Status = UploadStatuses.COMPLETE.ToString(),
            };
        }

        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = _lhs.CompletedAt,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.True(_lhs.Equals(rhs));
            Assert.Equal(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_Match_When_CompletedAt_Is_Null()
        {
            // Arrange
            _lhs.CompletedAt = null;

            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = null,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.True(_lhs.Equals(rhs));
            Assert.Equal(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_Id_Mismatch()
        {
            var rhs = new UploadDto
            {
                Id = 999,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = null,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_UploadIdentifier_Mismatch()
        {
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = "xyz",
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = null,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_CreatedAt_Mismatch()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = DateTime.UtcNow,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = _lhs.CompletedAt,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_Publisher_Mismatch()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = "Someone else",
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = _lhs.CompletedAt,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_ParticipantsUploaded_Mismatch()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = 1000,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = _lhs.CompletedAt,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_ErrorMessage_Mismatch()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = "No error to see here",
                CompletedAt = _lhs.CompletedAt,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_CompletedAt_Mismatch()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = DateTime.UtcNow,
                Status = _lhs.Status,
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_Status_Mismatch()
        {
            // Arrange
            var rhs = new UploadDto
            {
                Id = _lhs.Id,
                UploadIdentifier = _lhs.UploadIdentifier,
                CreatedAt = _lhs.CreatedAt,
                Publisher = _lhs.Publisher,
                ParticipantsUploaded = _lhs.ParticipantsUploaded,
                ErrorMessage = _lhs.ErrorMessage,
                CompletedAt = _lhs.CompletedAt,
                Status = UploadStatuses.FAILED.ToString(),
            };

            // Act / Assert
            Assert.False(_lhs.Equals(rhs));
            Assert.NotEqual(_lhs.GetHashCode(), rhs.GetHashCode());
        }
    }
}
