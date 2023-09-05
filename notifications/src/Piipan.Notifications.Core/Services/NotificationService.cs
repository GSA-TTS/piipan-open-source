using Microsoft.Extensions.Logging;
using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationPublish _notificationPublish;
        public ILogger<NotificationService> _logger;
        private readonly IViewRenderService _viewRenderService;

        public NotificationService(INotificationPublish notificationPublish, IViewRenderService viewRenderService, ILogger<NotificationService> logger)
        {
            _notificationPublish = notificationPublish;
            _viewRenderService = viewRenderService;
            _logger = logger;
        }

        public async Task<bool> PublishNotificationOnMatchCreation(NotificationRecord notificationRecord)
        {
            
            string subject = $"{{0}}: NAC Match {notificationRecord.MatchEmailDetails.MatchId} with {{1}}";

            // Send emails only if the state is enabled. nac-1902
            if (!notificationRecord.IsInitiatingStateEnabled || !notificationRecord.IsMatchingStateEnabled) //No Email is sent out if the Initiating State is not Enabled 
                return false;

            //Send Notofication to Iniatiating State and Matching State.  Passing Iniatiating state and Matching state templete respectively.
            notificationRecord.MatchEmailDetails.IsInitiatingState = true;
            var emailbodyIS = await _viewRenderService.GenerateMessageContent("MatchEmail.cshtml", notificationRecord.MatchEmailDetails);
            var emailsubjectIS = string.Format(subject, "Initiating State", notificationRecord.MatchEmailDetails.MatchingState);

            // Render the matching state email
            notificationRecord.MatchEmailDetails.IsInitiatingState = false;
            var emailbodyMS = await _viewRenderService.GenerateMessageContent("MatchEmail.cshtml", notificationRecord.MatchEmailDetails);
            var emailsubjectMS = string.Format(subject, "Matching State", notificationRecord.MatchEmailDetails.InitState);

            var resultIS = false;
            var resultMS = false;

            resultIS = await PublishNotifications(notificationRecord.InitiatingStateEmailRecipientsModel, emailbodyIS, emailsubjectIS);

            // Send emails only if the state is enabled. nac-1902
            if (notificationRecord.IsMatchingStateEnabled)
                resultMS = await PublishNotifications(notificationRecord.MatchingStateEmailRecipientsModel, emailbodyMS, emailsubjectMS);

            return resultMS || resultIS;
        }
        public async Task<bool> PublishNotificationOnMatchResEventsUpdate(NotificationRecord notificationRecord)
        {
            //Send Notofication to the other State. Todo  Refactor after getting the finialized template file .

            var emailbodyIS = await _viewRenderService.GenerateMessageContent("DispositionEmail.cshtml", notificationRecord);
            var emailsubjectIS = $"Updates made to NAC match {notificationRecord.MatchEmailDetails.MatchId}";
            return await PublishNotifications(notificationRecord.UpdateNotifyStateEmailRecipientsModel, emailbodyIS, emailsubjectIS);

        }
        public async Task<bool> PublishNotifications(EmailToModel emailToRecord, string emailbody, string emailsubject)
        {
            try
            {
                var emailModel = new EmailModel
                {
                    ToList = emailToRecord?.EmailTo.Split(',').ToList(),
                    ToCCList = emailToRecord?.EmailCcTo?.Split(',').ToList(),
                    ToBCCList = emailToRecord?.EmailBccTo?.Split(',').ToList(),
                    Body = emailbody,
                    Subject = emailsubject,
                    From = string.Empty,
                };
                // Publish to the queue.
                await _notificationPublish.PublishEmail(emailModel);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, String.Format("Failed to publish Notification for the state {0} and email subject {1}", emailToRecord?.EmailTo, emailsubject));
                return false;
            }
        }

    }
}