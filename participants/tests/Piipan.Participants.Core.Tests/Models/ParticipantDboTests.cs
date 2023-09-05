using System;
using System.Collections.Generic;
using Piipan.Participants.Core.Models;
using Piipan.Shared.API.Utilities;
using Xunit;

namespace Piipan.Participants.Core.Tests.Models
{
    public class ParticipantDboTests
    {
        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };

            // Act / Assert
            Assert.False(lhs.Equals(null));
        }

        [Fact]
        public void Equals_WrongType()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new
            {
                value = 5
            };

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
        }

        [Fact]
        public void Equals_HashCode_LdsHashMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash + "2",
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_CaseIdMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId + "2",
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_ParticipantIdMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId + "2",
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_ParticipantClosingDateMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate + TimeSpan.FromDays(1),
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_RecentBenefitsMonthsMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = new List<DateRange> {new DateRange( DateTime.UtcNow, DateTime.UtcNow )},
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_VulnerableIndividualMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = !lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_UploadIdMismatch()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId + 1
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var lhs = new ParticipantDbo
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitIssuanceDates = new List<DateRange>(),
                VulnerableIndividual = false,
                UploadId = 1
            };
            var rhs = new ParticipantDbo
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitIssuanceDates = lhs.RecentBenefitIssuanceDates,
                VulnerableIndividual = lhs.VulnerableIndividual,
                UploadId = lhs.UploadId
            };
            

            // Act / Assert
            Assert.True(lhs.Equals(rhs));
            Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
        }
    }
}