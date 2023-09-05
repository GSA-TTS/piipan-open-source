using Piipan.Match.Api.Models.Resolution;

namespace Piipan.Notification.Common.Models
{
    public class DispositionModel
    {
        public string MatchId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string InitState { get; set; }
        public string MatchingState { get; set; }

        public Disposition InitStateDisposition { get; set; } = new();
        public Disposition MatchingStateDisposition { get; set; } = new();

        public string Status { get; set; }
    }
}