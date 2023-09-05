using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Services
{
    public class NotificationServiceTest
    {

        [Fact]
        public async Task PublishMessageFromTemplateTest()
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
        public async Task PublishMessageFromTemplateTest_Exception()
        {
            var notificationRecord = new NotificationRecord { };
            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();
            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));
            var logger = new Mock<ILogger<NotificationService>>();
            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            // Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => service.PublishNotificationOnMatchCreation(notificationRecord));
        }

        [Fact]
        public async Task PublishMessageFromMatchResEventsUpdateTemplateTest()
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
                UpdateNotifyStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@nac.com"
                }
            };
            string subject = "Updates made to NAC match foo";
            string body = "Test Body";

            var emailModel = new EmailModel
            {

                ToList = "ea@nac.com".Split(',').ToList(),
                ToCCList = null,
                ToBCCList = null,
                Body = body,
                Subject = subject,
                From = "",
            };

            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();

            viewRenderService
                .Setup(m => m.GenerateMessageContent("DispositionEmail.cshtml", It.IsAny<NotificationRecord>()))
                .Returns(Task.FromResult(body));

            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));

            var logger = new Mock<ILogger<NotificationService>>();

            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            var ret = await service.PublishNotificationOnMatchResEventsUpdate(notificationRecord);
            // Assert
            Assert.True(ret);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModel.Body && p.Subject == emailModel.Subject && p.ToList[0] == "ea@nac.com")), Times.Once);
        }

        [Fact]
        public async Task PublishMessageValidateEmailIdsTest()
        {

            Environment.SetEnvironmentVariable("SmtpCcEmail", "test-cc@example.com");
            Environment.SetEnvironmentVariable("SmtpBccEmail", "test-bcc@example.com");
            Environment.SetEnvironmentVariable("SmtpFromEmail", "noreply@example.com");

            var notificationRecord = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = "http://test.com",
                },
                UpdateNotifyStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@nac.com",
                    EmailCcTo = "test-cc-ea@nac.com"
                }
            };
            string subject = "Updates made to NAC match foo";
            string body = "Test Body";

            var emailModel = new EmailModel
            {

                ToList = "ea@nac.com".Split(',').ToList(),
                ToCCList = "test-cc-ea@nac.com".Split(',').ToList(),
                ToBCCList = "test-bcc@example.com".Split(',').ToList(),
                Body = body,
                Subject = subject,
                From = "noreply@example.com",
            };

            var notificatioPublish = new Mock<INotificationPublish>();
            var viewRenderService = new Mock<IViewRenderService>();

            viewRenderService
                .Setup(m => m.GenerateMessageContent("DispositionEmail.cshtml", It.IsAny<NotificationRecord>()))
                .Returns(Task.FromResult(body));

            notificatioPublish
                .Setup(m => m.PublishEmail(It.IsAny<EmailModel>()));

            var logger = new Mock<ILogger<NotificationService>>();

            var service = new NotificationService(notificatioPublish.Object, viewRenderService.Object, logger.Object);
            var ret = await service.PublishNotificationOnMatchResEventsUpdate(notificationRecord);
            // Assert
            Assert.True(ret);

            notificatioPublish.Verify(m => m.PublishEmail(It.Is<EmailModel>(p => p.Body == emailModel.Body &&
                                                                                 p.Subject == emailModel.Subject &&
                                                                                 p.ToList[0] == "ea@nac.com" &&
                                                                                 p.ToCCList[0] == "test-cc-ea@nac.com"
                                                                                 )), Times.Once);
        }
    }
}
