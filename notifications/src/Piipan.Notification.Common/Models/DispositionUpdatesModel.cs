namespace Piipan.Notification.Common.Models
{
    public record DispositionUpdatesModel
    {
        public HashSet<string> InitStateUpdates { get; set; } = new HashSet<string>();
        public HashSet<string> MatchingStateUpdates { get; set; } = new HashSet<string>();
    }
}