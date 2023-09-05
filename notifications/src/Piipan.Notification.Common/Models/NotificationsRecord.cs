namespace Piipan.Notification.Common.Models
{
    public class NotificationRecord
    {
        public MatchEmailModel MatchEmailDetails { get; set; }
        public EmailToModel InitiatingStateEmailRecipientsModel { get; set; }
        public EmailToModel MatchingStateEmailRecipientsModel { get; set; }
        public DispositionModel MatchResEvent { get; set; }
        public DispositionUpdatesModel DispositionUpdates { get; set; }
        public EmailToModel UpdateNotifyStateEmailRecipientsModel { get; set; }
        public bool IsMatchingStateEnabled { get; set; }
        public bool IsInitiatingStateEnabled { get; set; }
        public NotificationRecord()
        {
        }
    }
}