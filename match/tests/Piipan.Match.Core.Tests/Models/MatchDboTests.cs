using System;
using Piipan.Match.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Models
{
    public class MatchDboTests
    {
        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };

            // Act / Assert
            Assert.False(record.Equals(null));
        }

        [Fact]
        public void Equals_WrongType()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var nRecord = new
            {
                value = 1
            };

            // Act / Assert
            Assert.False(record.Equals(nRecord));
        }

        [Fact]
        public void Equals_HashCode_MatchIdMismatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var recordMismatch = new MatchDbo
            {
                MatchId = record.MatchId + "1",
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType
            };

            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_InitiatorMismatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var recordMismatch = new MatchDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator + "b",
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType
            };

            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_StatesMismatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var recordMismatch = new MatchDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = new string[] { "a", "c" },
                Hash = record.Hash,
                HashType = record.HashType
            };

            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_HashMismatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var recordMismatch = new MatchDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash + "a",
                HashType = record.HashType
            };

            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_HashTypeMismatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var recordMismatch = new MatchDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType + "y"
            };

            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_InputMatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Input = "{}"
            };
            var recordMatch = new MatchDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Input = "{[]}"
            };

            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_DataMatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Data = "{}"
            };
            var recordMatch = new MatchDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Data = "{[]}"
            };

            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_CreatedAtMatch()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                CreatedAt = DateTime.Now,
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var a = DateTime.Now.AddDays(1);
            var recordMatch = new MatchDbo
            {
                MatchId = record.MatchId,
                CreatedAt = ((DateTime)record.CreatedAt).AddDays(1),
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType
            };

            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }

        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var record = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };
            var recordMatch = new MatchDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t"
            };

            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }
    }
}
