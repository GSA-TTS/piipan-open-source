namespace Piipan.Notifications.Core.Extensions
{
    /// <summary>
    /// Utility class holding configuration from Notification function app.
    /// This would contain Email Ids and configuration to SendEmail or to Log.
    /// </summary>
    public class EmailDelivery
    {
        public bool Enabled { get; set; } = true;
        public string EmailCc { get; set; }
        public string EmailBcc { get; set; }
        public string EmailFrom { get; set; }
    }
}
