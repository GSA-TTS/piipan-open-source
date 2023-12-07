using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Services;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.States.Core.Service;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchResNotifyServiceTests
    {
        private string[] enabledStateList = { "ea", "eb" };
        private string[] enabledStateShortList = { "eb" };
        private Mock<INotificationService> NotificationServiceMock()
        {
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.PublishNotificationOnMatchResEventsUpdate(
                It.IsAny<NotificationRecord>())).Returns(Task.FromResult(true));
            return mock;
        }

        private Mock<IStateInfoService> StateInfoDaoMock()
        {
            var stateInfoDao = new Mock<IStateInfoService>();
            stateInfoDao
                .Setup(r => r.GetDecryptedStates())
                    .ReturnsAsync(new List<StateInfoDbo>()
                    {
                    new StateInfoDbo() { Id = "1", State = "Echo Alpha", StateAbbreviation = "ea" , Email = "ea-test@agency.example"},
                    new StateInfoDbo() { Id = "2", State = "Echo Bravo", StateAbbreviation = "eb" , Email = "eb-test@agency.example", EmailCc = "eb-test-cc@Piipan.gov"  },
                    });
            return stateInfoDao;
        }


        private MatchDetailsDto BeforeMatchUpdate = new MatchDetailsDto()
        {
            MatchId = "foo",
            Initiator = "ea",
            States = new string[] { "ea", "eb" },
            Dispositions = new Disposition[]
            {
                new Disposition
                {
                    State = "ea"
                },
                new Disposition
                {
                    State = "eb"
                }
            },
            CreatedAt = new DateTime(2022, 9, 1),
            Status = "Open"
        };

        private MatchDetailsDto AfterMatchUpdate = new MatchDetailsDto()
        {
            MatchId = "foo",
            Initiator = "ea",
            States = new string[] { "ea", "eb" },
            Dispositions = new Disposition[]
            {
                new Disposition
                {
                    State = "ea"
                },
                new Disposition
                {
                    State = "eb"
                }
            },
            CreatedAt = new DateTime(2022, 9, 1),
            Status = "Open"
        };

        [Fact]
        public async void ValidateNotificationSendToNoStatesWhenNeitherHaveValidUpdates()
        {
            Environment.SetEnvironmentVariable("EnabledStates", "ea,eb");
            AfterMatchUpdate.Dispositions[0].VulnerableIndividual = false;
            AfterMatchUpdate.Dispositions[1].VulnerableIndividual = false;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
                n.DispositionUpdates.InitStateUpdates.Count == 1 &&
                    n.DispositionUpdates.MatchingStateUpdates.Count == 1 &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                    n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                    n.MatchEmailDetails.InitState == "Echo Alpha" &&
                    n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Never);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Never);
        }

        [Fact]
        public async void ValidateNotificationSendToBothStatesWhenBothHaveValidUpdates()
        {
            Environment.SetEnvironmentVariable("EnabledStates", "ea,eb");
            AfterMatchUpdate.Dispositions[0].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[0].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[0].VulnerableIndividual = true;
            AfterMatchUpdate.Dispositions[1].FinalDisposition = "disposition";
            AfterMatchUpdate.Dispositions[1].FinalDispositionDate = DateTime.Now;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
                n.DispositionUpdates.InitStateUpdates.Count == 3 &&
                    n.DispositionUpdates.MatchingStateUpdates.Count == 2 &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.FinalDisposition)) &&
                    n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.FinalDispositionDate)) &&
                    n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                    n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                    n.MatchEmailDetails.InitState == "Echo Alpha" &&
                    n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Once);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Once);
        }

        [Fact]
        public async void ValidateNotificationSendToInitiatingStateWhenMatchingStateHasValidUpdates()
        {
            Environment.SetEnvironmentVariable("EnabledStates", "ea,eb");
            AfterMatchUpdate.Dispositions[0].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[0].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[0].VulnerableIndividual = true;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
                n.DispositionUpdates.InitStateUpdates.Count == 3 &&
                    n.DispositionUpdates.MatchingStateUpdates.Count == 0 &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                    n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                    n.MatchEmailDetails.InitState == "Echo Alpha" &&
                    n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Once);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Never);
        }

        [Fact]
        public async void ValidateNotificationSendToMatchingStateWhenInitiatingStateHasValidUpdates()
        {
            Environment.SetEnvironmentVariable("EnabledStates", "ea,eb");
            AfterMatchUpdate.Dispositions[1].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[1].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[1].VulnerableIndividual = true;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
                n.DispositionUpdates.MatchingStateUpdates.Count == 3 &&
                    n.DispositionUpdates.InitStateUpdates.Count == 0 &&
                    n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                    n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                    n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                    n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                    n.MatchEmailDetails.InitState == "Echo Alpha" &&
                    n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Never);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Once);
        }

        [Fact]
        public async void ValidateNotification_Not_SendTo_InitiatingState_When_not_enabled_Initiating_State_updates()
        {
            // Initiating State is Enable and Matching state is Disabled.  Changes Made to Initiating:  No Email sent
            Environment.SetEnvironmentVariable("EnabledStates", "ea");
            AfterMatchUpdate.Dispositions[0].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[0].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[0].VulnerableIndividual = true;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
                n.DispositionUpdates.InitStateUpdates.Count == 3 &&
                    n.DispositionUpdates.MatchingStateUpdates.Count == 0 &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                    n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                    n.MatchEmailDetails.InitState == "Echo Alpha" &&
                    n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Never);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Never);
        }
        [Fact]
        public async void ValidateNotification_Not_SendTo_InitiatingState_When_not_enabled_Matching_State_Updates()
        {
            // Initiating State is Enable and Matching state is Disabled.  Changes Made to Matching State:  No Email sent
            Environment.SetEnvironmentVariable("EnabledStates", "ea");
            AfterMatchUpdate.Dispositions[1].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[1].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[1].VulnerableIndividual = true;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
             n.DispositionUpdates.MatchingStateUpdates.Count == 3 &&
                 n.DispositionUpdates.InitStateUpdates.Count == 0 &&
                 n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                 n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                 n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                 n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                 n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                 n.MatchEmailDetails.InitState == "Echo Alpha" &&
                 n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Never);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Never);
        }
        [Fact]
        public async void ValidateNotification_Not_SendTo_MatchingState_When_not_enabled_Initiating_State_updates()
        {
            // Matching State is Enable and Initiating state is Disabled.  Changes Made to Initiating:  No Email sent
            Environment.SetEnvironmentVariable("EnabledStates", "eb");
            AfterMatchUpdate.Dispositions[0].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[0].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[0].VulnerableIndividual = true;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
                n.DispositionUpdates.InitStateUpdates.Count == 3 &&
                    n.DispositionUpdates.MatchingStateUpdates.Count == 0 &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                    n.DispositionUpdates.InitStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                    n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                    n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                    n.MatchEmailDetails.InitState == "Echo Alpha" &&
                    n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Never);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Never);
        }
        [Fact]
        public async void ValidateNotification_Not_SendTo_MatchingState_When_not_enabled_Matching_State_Updates()
        {
            // Matching State is Enable and Initiating state is Disabled.  Changes Made to Matching State:  No Email sent
            Environment.SetEnvironmentVariable("EnabledStates", "eb");
            AfterMatchUpdate.Dispositions[1].InvalidMatch = true;
            AfterMatchUpdate.Dispositions[1].InvalidMatchReason = "Some reason";
            AfterMatchUpdate.Dispositions[1].VulnerableIndividual = true;

            // Act
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var api = new MatchResNotifyService(
                  stateRecordDao.Object,
                  notificationService.Object
            );

            await api.SendNotification(BeforeMatchUpdate, AfterMatchUpdate);

            Func<NotificationRecord, bool> correctInputs = (NotificationRecord n) =>
             n.DispositionUpdates.MatchingStateUpdates.Count == 3 &&
                 n.DispositionUpdates.InitStateUpdates.Count == 0 &&
                 n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.InvalidMatch)) &&
                 n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.InvalidMatchReason)) &&
                 n.DispositionUpdates.MatchingStateUpdates.Contains(nameof(Disposition.VulnerableIndividual)) &&
                 n.MatchResEvent.InitStateDisposition == AfterMatchUpdate.Dispositions[0] &&
                 n.MatchResEvent.MatchingStateDisposition == AfterMatchUpdate.Dispositions[1] &&
                 n.MatchEmailDetails.InitState == "Echo Alpha" &&
                 n.MatchEmailDetails.MatchingState == "Echo Bravo";

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "eb-test@agency.example"
                )), Times.Never);

            notificationService.Verify(
                r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(n =>
                    correctInputs(n) &&
                    n.UpdateNotifyStateEmailRecipientsModel != null && n.UpdateNotifyStateEmailRecipientsModel.EmailTo == "ea-test@agency.example"
                )), Times.Never);
        }

    }
}
