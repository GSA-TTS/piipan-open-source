using Piipan.Match.Core.Services;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchIdServiceTests
    {
        [Fact]
        public void GenerateId_IsCorrectLength()
        {
            // Arrange
            const int Length = 7;
            var idService = new MatchIdService();

            // Act
            var id = idService.GenerateId();

            // Assert
            Assert.Equal(Length, id.Length);
        }

        [Fact]
        public void GenerateId_UsesAllowedCharacters()
        {
            // Arrange
            const string Chars = "23456789BCDFGHJKLMNPQRSTVWXYZ";
            var idService = new MatchIdService();

            // Act
            var id = idService.GenerateId();

            // Assert
            Assert.Matches($"^[{Chars}]+$", id);
        }
    }
}
