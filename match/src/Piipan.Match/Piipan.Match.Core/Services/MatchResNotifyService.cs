using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Shared.Extensions;
using Piipan.States.Core.Service;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// This service will send out notifications for Match Res Events, if they meet criteria for sending notifications.
    /// Then it will mark them as notified and they will not get picked up in future requests.
    /// </summary>
    public class MatchResNotifyService : IMatchResNotifyService
    {
        private readonly IStateInfoService _stateInfoService;
        private readonly INotificationService _notificationService;
        private readonly string _replyToEmail;


        public MatchResNotifyService(
            IStateInfoService stateInfoService,
            INotificationService notificationService)
        {
            _stateInfoService = stateInfoService;
            _notificationService = notificationService;
            _replyToEmail = Environment.GetEnvironmentVariable("SmtpReplyToEmail");
        }

        /// <summary>
        /// Sends a Match notification out if the updates warrant a notification.
        /// </summary>
        /// <param name="matchResRecordBeforeUpdate">The match before the update</param>
        /// <param name="matchResRecordAfterUpdate">The match after the update</param>
        /// <returns></returns>
        public async Task SendNotification(MatchDetailsDto matchResRecordBeforeUpdate, MatchDetailsDto matchResRecordAfterUpdate)
        {
            //Send Notification to both initiating state and Matching State.
            // Send template data for any email template which is created based on the requirements.
            // The below logic might change based on the template data for requirements.
            //In future we might end up consolidating the logic based on requirements.
            //Send Email to the state which is not actorState.
            var states = await _stateInfoService.GetDecryptedStates();
            var initState = states?.Where(n => string.Compare(n.StateAbbreviation, matchResRecordAfterUpdate.States[0], true) == 0).FirstOrDefault();
            var matchingState = states?.Where(n => string.Compare(n.StateAbbreviation, matchResRecordAfterUpdate.States[1], true) == 0).FirstOrDefault();

            // Getting Enabled state list 
            string enabledStates = Environment.GetEnvironmentVariable("EnabledStates")?.ToLower();
            var enabledStatesList = enabledStates?.Split(',') ?? new string[0];

            // Send emails only if the state is enabled. Piipan-1902
            if (!enabledStatesList.Contains(matchingState.StateAbbreviation.ToLower()) || !enabledStatesList.Contains(initState.StateAbbreviation.ToLower())) //Send Email only if the Initiating state or Matching State is Enabled
                return;

            //Getting the Final Disposition from the Metrics object which is the latest record.
            // Compare with matchResRecordAfterUpdate and matchResRecord to know what all things got updated.
            //Once we get the Type 2 notification mark-ups then need to get the differences marked in the email.
            //Currently getting all the attributes and will drop few based on the Mark-up
            DispositionModel dispositionModel = GetDispositionModel(matchResRecordAfterUpdate);
            DispositionModel dispositionModeBeforeUpdate = GetDispositionModel(matchResRecordBeforeUpdate);

            //Business logic for trigger
            // Get the list of the things got updated
            DispositionUpdatesModel dispositionUpdates = GetDispositionUpdates(dispositionModeBeforeUpdate, dispositionModel);
            // If Anything is updated by Initiating state then send email to Matching stage and Vice versa.

            var queryToolUrl = Environment.GetEnvironmentVariable("QueryToolUrl");
            var matchRecord = new MatchEmailModel()
            {
                MatchId = matchResRecordBeforeUpdate.MatchId,
                InitState = initState?.State,
                MatchingState = matchingState?.State,
                MatchingUrl = $"{queryToolUrl}/match/{matchResRecordBeforeUpdate.MatchId}",
                InitialActionBy = DateTime.UtcNow.ToEasternTime().AddDays(10),  // Converting to Eastern Time since we don't know the end user's timezone. See Piipan-1613
                CreateDate = matchResRecordAfterUpdate.CreatedAt,
                ReplyToEmail = _replyToEmail
            };

            // If there are valid initiating state changes, send an update to the matching state
            if (IsValidForNotification(dispositionModeBeforeUpdate.InitStateDisposition, dispositionModel.InitStateDisposition, dispositionUpdates.InitStateUpdates))
            {
                NotificationRecord notificationRecord = new NotificationRecord
                {
                    DispositionUpdates = dispositionUpdates,
                    UpdateNotifyStateEmailRecipientsModel = new EmailToModel
                    {
                        EmailTo = matchingState.Email,
                        EmailCcTo = matchingState.EmailCc,
                    },
                    MatchingStateEmailRecipientsModel = new EmailToModel //Model is used in formatting Notification Templates
                    {
                        EmailTo = matchingState.Email,
                        EmailCcTo = matchingState.EmailCc,
                    },
                    MatchResEvent = dispositionModel,
                    MatchEmailDetails = matchRecord
                };
                await _notificationService.PublishNotificationOnMatchResEventsUpdate(notificationRecord); //Publishing Email for Initiating & Matching State:  Based on the requirement
            }

            // If there are valid matching state changes, send an update to the initiating state
            if (IsValidForNotification(dispositionModeBeforeUpdate.MatchingStateDisposition, dispositionModel.MatchingStateDisposition, dispositionUpdates.MatchingStateUpdates))
            {
                NotificationRecord notificationRecord = new NotificationRecord
                {
                    DispositionUpdates = dispositionUpdates,
                    UpdateNotifyStateEmailRecipientsModel = new EmailToModel
                    {
                        EmailTo = initState?.Email,
                        EmailCcTo = initState?.EmailCc
                    },
                    InitiatingStateEmailRecipientsModel = new EmailToModel //Model is used in formatting Notification Templates
                    {
                        EmailTo = initState?.Email,
                        EmailCcTo = matchingState.EmailCc
                    },
                    MatchResEvent = dispositionModel,
                    MatchEmailDetails = matchRecord
                };

                await _notificationService.PublishNotificationOnMatchResEventsUpdate(notificationRecord); //Publishing Email for Initiating & Matching State:  Based on the requirement
            }
        }

        private DispositionModel GetDispositionModel(MatchDetailsDto matchResRecord)
        {
            var initiatingStateDisposition = matchResRecord.Dispositions.FirstOrDefault(r => r.State.Equals(matchResRecord.Initiator, StringComparison.OrdinalIgnoreCase));
            var matchingStateDisposition = matchResRecord.Dispositions.FirstOrDefault(r => !r.State.Equals(matchResRecord.Initiator, StringComparison.OrdinalIgnoreCase));
            return new DispositionModel()
            {
                MatchId = matchResRecord.MatchId,
                InitState = matchResRecord.Initiator,
                MatchingState = matchResRecord.States.FirstOrDefault(n => !n.Equals(matchResRecord.Initiator, StringComparison.OrdinalIgnoreCase)),
                CreatedAt = matchResRecord.CreatedAt,
                Status = matchResRecord.Status,
                InitStateDisposition = initiatingStateDisposition,
                MatchingStateDisposition = matchingStateDisposition
            };
        }

        private bool IsValidForNotification(Disposition beforeDisposition, Disposition afterDisposition, HashSet<string> dispositionUpdates)
        {
            if (((afterDisposition.VulnerableIndividual == true || beforeDisposition.VulnerableIndividual == true) && afterDisposition.VulnerableIndividual != beforeDisposition.VulnerableIndividual) ||
                ((afterDisposition.InvalidMatch == true || beforeDisposition.InvalidMatch == true) && afterDisposition.InvalidMatch != beforeDisposition.InvalidMatch) ||
                (dispositionUpdates.Contains(nameof(Disposition.InitialActionAt)) || dispositionUpdates.Contains(nameof(Disposition.InitialActionTaken))) ||
                (dispositionUpdates.Contains(nameof(Disposition.FinalDispositionDate)) || dispositionUpdates.Contains(nameof(Disposition.FinalDisposition))))
            {
                return true;
            }
            return false;
        }


        private DispositionUpdatesModel GetDispositionUpdates(DispositionModel dispositionModeBeforeUpdate, DispositionModel dispositionModel)
        {
            DispositionUpdatesModel dispositionUpdates = new DispositionUpdatesModel();
            foreach (PropertyInfo info in dispositionModel.InitStateDisposition.GetType().GetProperties())
            {
                object propValueA = info.GetValue(dispositionModeBeforeUpdate.InitStateDisposition, null);
                object propValueB = info.GetValue(dispositionModel.InitStateDisposition, null);
                if (!Equals(propValueA, propValueB))
                {
                    dispositionUpdates.InitStateUpdates.Add(info.Name);
                }
            }
            foreach (PropertyInfo info in dispositionModel.MatchingStateDisposition.GetType().GetProperties())
            {
                object propValueA = info.GetValue(dispositionModeBeforeUpdate.MatchingStateDisposition, null);
                object propValueB = info.GetValue(dispositionModel.MatchingStateDisposition, null);
                if (!Equals(propValueA, propValueB))
                {
                    dispositionUpdates.MatchingStateUpdates.Add(info.Name);
                }
            }
            return dispositionUpdates;
        }
    }
}