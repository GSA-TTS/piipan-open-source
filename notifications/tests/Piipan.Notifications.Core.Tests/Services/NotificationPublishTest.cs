using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationPublishTest
    {
        [Fact]
        public async void NotificationPublish_Sucess()
        {
            var notificationRecord = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = "http://test.com",
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@nac.com"
                },
                IsInitiatingStateEnabled = true,
                IsMatchingStateEnabled = true
            };
            string subIS = "Initiating State: NAC Match foo with eb";
            string bodyIS = "IS: Test Body";
            string subMS = "Matching State: NAC Match foo with ea";
            string bodyMS = "MS: Test Body";

            var emailModelIS = new EmailModel
            {

                ToList = "ea@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyIS,
                Subject = subIS,
                From = "",
            };
            var emailModelMS = new EmailModel
            {

                ToList = "eb@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyMS,
                Subject = subMS,
                From = "",
            };


            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmail.cshtml", It.Is<MatchEmailModel>(n => n.IsInitiatingState)))
                .Returns(Task.FromResult(bodyIS));

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmail.cshtml", It.Is<MatchEmailModel>(n => !n.IsInitiatingState)))
                .Returns(Task.FromResult(bodyMS));

            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));

            var logger = new Mock<ILogger<NotificationService>>();

            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            var ret = await service.PublishNotificationOnMatchCreation(notificationRecord);
            // Assert
            Assert.True(ret);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelIS.Body && p.Subject == emailModelIS.Subject && p.ToList[0] == "ea@nac.com")), Times.Once);
            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelMS.Body && p.Subject == emailModelMS.Subject && p.ToList[0] == "eb@nac.com")), Times.Once);




        }
        [Fact]
        public async void NotificationPublish_Failed()
        {
            Environment.SetEnvironmentVariable("EventGridNotifyEndPoint", null);
            Environment.SetEnvironmentVariable("EventGridNotifyKeyString", null);
            var logger = new Mock<ILogger<EmailModel>>();
            var notificationPublish = new NotificationPublish(logger.Object);
            Mock<EventGridPublisherClient> publisherClientMock = new Mock<EventGridPublisherClient>();

            notificationPublish._client = publisherClientMock.Object;
            string emails = "test@test.com,test1@test.com";
            var emailModel = new EmailModel
            {

                ToList = emails.Split(',').ToList(),
                ToCCList = emails.Split(',').ToList(),
                ToBCCList = emails.Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };
            await notificationPublish.PublishEmail(emailModel);
            publisherClientMock.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(s => s.EventType == "Publish to the queue"), default));


            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EventGridNotifyEndPoint and EventGridNotifyKeyString environment variables are not set")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }
        [Fact]
        public async void NotificationPublish_Sucess_No_Notification_For_Not_EnabledState_Initiating_State()
        {
            var notificationRecord = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = "http://test.com",
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@nac.com"
                },
                IsInitiatingStateEnabled = false
            };
            string subIS = "Initiating State: NAC Match foo with eb";
            string bodyIS = "IS: Test Body";
            string subMS = "Matching State: NAC Match foo with ea";
            string bodyMS = "MS: Test Body";

            var emailModelIS = new EmailModel
            {

                ToList = "ea@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyIS,
                Subject = subIS,
                From = "",
            };
            var emailModelMS = new EmailModel
            {

                ToList = "eb@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyMS,
                Subject = subMS,
                From = "",
            };


            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmail.cshtml", It.Is<MatchEmailModel>(n => n.IsInitiatingState)))
                .Returns(Task.FromResult(bodyIS));

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmail.cshtml", It.Is<MatchEmailModel>(n => !n.IsInitiatingState)))
                .Returns(Task.FromResult(bodyMS));

            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));

            var logger = new Mock<ILogger<NotificationService>>();

            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            var ret = await service.PublishNotificationOnMatchCreation(notificationRecord);
            // Assert
            Assert.False(ret);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelIS.Body && p.Subject == emailModelIS.Subject && p.ToList[0] == "ea@nac.com")), Times.Never);
            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelMS.Body && p.Subject == emailModelMS.Subject && p.ToList[0] == "eb@nac.com")), Times.Never);

        }
        [Fact]
        public async void NotificationPublish_Sucess_No_Notification_For_Not_EnabledState_Matching_State()
        {
            var notificationRecord = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = "http://test.com",
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@nac.com"
                },
                IsMatchingStateEnabled = false,
                IsInitiatingStateEnabled = true
            };
            var notificationRecordISDisabled = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = "http://test.com",
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@nac.com"
                },
                IsMatchingStateEnabled = false,
                IsInitiatingStateEnabled = true
            };
            string subIS = "Initiating State: NAC Match foo with eb";
            string bodyIS = "IS: Test Body";
            string subMS = "Matching State: NAC Match foo with ea";
            string bodyMS = "MS: Test Body";

            var emailModelIS = new EmailModel
            {

                ToList = "ea@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyIS,
                Subject = subIS,
                From = "",
            };
            var emailModelMS = new EmailModel
            {

                ToList = "eb@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = bodyMS,
                Subject = subMS,
                From = "",
            };


            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmail.cshtml", It.Is<MatchEmailModel>(n => n.IsInitiatingState)))
                .Returns(Task.FromResult(bodyIS));

            viewRenderService
                .Setup(m => m.GenerateMessageContent("MatchEmail.cshtml", It.Is<MatchEmailModel>(n => !n.IsInitiatingState)))
                .Returns(Task.FromResult(bodyMS));

            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));

            var logger = new Mock<ILogger<NotificationService>>();

            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            var ret = await service.PublishNotificationOnMatchCreation(notificationRecord);
           
            // Assert
            Assert.False(ret);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelIS.Body && p.Subject == emailModelIS.Subject && p.ToList[0] == "ea@nac.com")), Times.Never);
            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelMS.Body && p.Subject == emailModelMS.Subject && p.ToList[0] == "eb@nac.com")), Times.Never);

            // Call IS disabled conditition

            var retISDisabled = await service.PublishNotificationOnMatchCreation(notificationRecordISDisabled);

            //Assert

            Assert.False(retISDisabled);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelIS.Body && p.Subject == emailModelIS.Subject && p.ToList[0] == "ea@nac.com")), Times.Never);
            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModelMS.Body && p.Subject == emailModelMS.Subject && p.ToList[0] == "eb@nac.com")), Times.Never);

        }

    }

}
