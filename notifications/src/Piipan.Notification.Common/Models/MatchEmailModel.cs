namespace Piipan.Notification.Common.Models
{
    public class MatchEmailModel
    {
        public string MatchId { get; set; }
        public string InitState { get; set; }
        public string MatchingState { get; set; }
        public string MatchingUrl { get; set; }
        public bool IsInitiatingState { get; set; }
        public DateTime InitialActionBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ReplyToEmail { get; set; }
        public bool IsMatchingStateEnabled { get; set; }
        public bool IsInitiatingStateEnabled { get; set; }
    }
}