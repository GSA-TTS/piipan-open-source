using Piipan.Match.Api.Models.Resolution;
using Piipan.Notification.Common.Models;
using Xunit;
namespace Piipan.Notifications.Core.Tests.Models
{
    public class DispositionUpdatesModelTest
    {
        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var record = new DispositionUpdatesModel();

            // Act / Assert
            Assert.Empty(record.InitStateUpdates);
            Assert.Empty(record.MatchingStateUpdates);
        }

        [Fact]
        public void CanMarkFieldsUpdated_InitState()
        {
            // Arrange
            var record = new DispositionUpdatesModel();

            // Act
            record.InitStateUpdates.Add(nameof(Disposition.InitialActionTaken));
            record.InitStateUpdates.Add(nameof(Disposition.InvalidMatch));

            // Act / Assert
            Assert.Contains(nameof(Disposition.InitialActionTaken), record.InitStateUpdates);
            Assert.Contains(nameof(Disposition.InvalidMatch), record.InitStateUpdates);
            Assert.Empty(record.MatchingStateUpdates);
        }

        [Fact]
        public void CanMarkFieldsUpdated_MatchingState()
        {
            // Arrange
            var record = new DispositionUpdatesModel();

            // Act
            record.MatchingStateUpdates.Add(nameof(Disposition.InitialActionTaken));
            record.MatchingStateUpdates.Add(nameof(Disposition.InvalidMatch));

            // Act / Assert
            Assert.Contains(nameof(Disposition.InitialActionTaken), record.MatchingStateUpdates);
            Assert.Contains(nameof(Disposition.InvalidMatch), record.MatchingStateUpdates);
            Assert.Empty(record.InitStateUpdates);
        }
    }
}
