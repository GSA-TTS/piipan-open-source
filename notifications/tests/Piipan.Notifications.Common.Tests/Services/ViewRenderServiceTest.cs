using Moq;
using Piipan.Notification.Common.Models;
using Piipan.Shared.Extensions;
using Xunit;

namespace Piipan.Notifications.Common.Tests.Services
{
    public class ViewRenderServiceTest : BasePageTest
    {

        [Fact]
        public async Task GenerateMessageContentCreateMatchBodyISTest()
        {
            var initialActionBy = DateTime.UtcNow.ToEasternTime().AddDays(10);
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                    IsInitiatingState = true,
                    InitialActionBy = initialActionBy,
                    ReplyToEmail = "helpdesk@agency.example"
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("MatchEmail.cshtml", notification.MatchEmailDetails);
            // Assert
            Assert.Contains("foo", emailBody);
            Assert.Contains("<p>This is an automated notification and we are unable to accept replies. If you need additional assistance, please contact us at <a href=\"mailto:helpdesk@agency.example?subject=NAC%20Match%20foo\">helpdesk@agency.example</a>.</p>", emailBody);
            Assert.Contains("<p>A NAC match has been found with your state.</p>", emailBody);
            Assert.Contains($"<p><em>Initial action to resolve this match must be taken and communicated to the other State agency <strong>by {initialActionBy.ToShortDateString()}</strong>", emailBody);
        }
        [Fact]
        public async Task GenerateMessageContentCreateMatchBodyMSTest()
        {
            var initialActionBy = DateTime.UtcNow.ToEasternTime().AddDays(10);
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                    IsInitiatingState = false,
                    InitialActionBy = initialActionBy,
                    ReplyToEmail = "helpdesk@agency.example"
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("MatchEmail.cshtml", notification.MatchEmailDetails);
            // Assert
            Assert.Contains("foo", emailBody);
            Assert.Contains("<p>This is an automated notification and we are unable to accept replies. If you need additional assistance, please contact us at <a href=\"mailto:helpdesk@agency.example?subject=NAC%20Match%20foo\">helpdesk@agency.example</a>.</p>", emailBody);
            Assert.Contains("<p>A NAC match has been found with your state.</p>", emailBody);
            Assert.Contains($"<p><em>Initial action to resolve this match must be taken and communicated to the other State agency <strong>by {initialActionBy.ToShortDateString()}</strong>", emailBody);
        }

        [Fact]
        public async Task GenerateMessageContentUpdateMatchResEventRecoredBodyTest_ForInitiatingState()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchResEvent = new DispositionModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingStateDisposition = new Piipan.Match.Api.Models.Resolution.Disposition
                    {
                        InvalidMatch = true,
                        InvalidMatchReason = "Other",
                        OtherReasoningForInvalidMatch = "Test for other invalid match",
                        FinalDisposition = "Benefits Approved",
                        FinalDispositionDate = DateTime.Now
                    }
                },
                MatchEmailDetails = new MatchEmailModel
                {
                    CreateDate = DateTime.UtcNow,
                    MatchId = "foo",
                    MatchingUrl = "https://www.example.com/match/foo",
                    ReplyToEmail = "helpdesk@agency.example"
                },
                DispositionUpdates = new DispositionUpdatesModel()
                {
                    MatchingStateUpdates = new HashSet<string> { "FinalDispositionDate", "FinalDisposition" }
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("DispositionEmail.cshtml", notification);
            // Assert
            Assert.Matches("<td class=\"fit\"><span class=\"match-change\">&#9679;</span></td>\\s+<td><strong>Final Disposition Taken:</strong> Benefits Approved", emailBody);

        }

        [Fact]
        public async Task GenerateMessageContentUpdateMatchResEventRecoredBodyTest_ForMatchingState()
        {
            // GenerateMessageContent 
            var notification = new NotificationRecord()
            {
                MatchResEvent = new DispositionModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    InitStateDisposition = new Piipan.Match.Api.Models.Resolution.Disposition
                    {
                        InvalidMatch = true,
                        InvalidMatchReason = "Other",
                        OtherReasoningForInvalidMatch = "Test for other invalid match",
                        FinalDisposition = "Benefits Terminated",
                        FinalDispositionDate = DateTime.Now
                    }
                },
                MatchEmailDetails = new MatchEmailModel
                {
                    CreateDate = DateTime.UtcNow,
                    MatchId = "foo",
                    InitState = "Echo Alpha",
                    MatchingState = "Echo Bravo",
                    MatchingUrl = "https://www.example.com/match/foo",
                    ReplyToEmail = "helpdesk@agency.example"
                },
                DispositionUpdates = new DispositionUpdatesModel()
                {
                    InitStateUpdates = new HashSet<string> { "InvalidMatchReason" }
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();


            var emailBody = await viewRenderService.GenerateMessageContent("DispositionEmail.cshtml", notification);
            // Assert
            // Assert first few paragraphs/sections
            Assert.Contains("<p>This is an automated notification and we are unable to accept replies. If you need additional assistance, please contact us at <a href=\"mailto:helpdesk@agency.example?subject=NAC%20Match%20foo\">helpdesk@agency.example</a>.</p>", emailBody);
            Assert.Contains("<p>An update was made to NAC Match ID <a href=\"https://www.example.com/match/foo\" title=\"View Match\">foo</a>.</p>", emailBody);
            Assert.Contains("<strong>Initiating State:</strong> Echo Alpha", emailBody);
            Assert.Contains("<strong>Matching State:</strong> Echo Bravo", emailBody);
            Assert.Contains("<strong>NAC Match ID:</strong> <a href=\"https://www.example.com/match/foo\" title=\"View Match\">foo</a>", emailBody);
            Assert.Contains("<strong>Match Record Created:</strong>", emailBody);
            Assert.Contains("<p><span class=\"match-change\">&#9679;</span> indicates most recently updated information</p>", emailBody);

            // Assert table contains a new change for the changed field
            Assert.Matches("<td class=\"fit\"><span class=\"match-change\">&#9679;</span></td>\\s+<td><strong>Invalid Match:</strong> Yes, Other: Test for other invalid match", emailBody);

            // Assert email has a reference to the usda logo
            Assert.Contains("<img src=\"cid:usda-img\" alt=\"USDA National Accuracy Clearinghouse\" />", emailBody);


        }

        [Fact]
        public async Task GenerateMessageContentEmailTemplateNotFoundTest()
        {
            var notification = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => viewRenderService.GenerateMessageContent("TestEmail.cshtml", notification.MatchEmailDetails));

        }
        [Fact]
        public async Task GenerateMessageContentPageModelNotFoundTest()
        {
            var notification = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => viewRenderService.GenerateMessageContent("TestEmail.cshtml", notification.MatchResEvent));

        }

        [Fact]
        public async Task GenerateMessageContentWrongModelTest()
        {
            var notification = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = "foo",
                    InitState = "ea",
                    MatchingState = "eb",
                    MatchingUrl = It.IsAny<string>(),
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "eb@Nac.com"
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = "ea@Nac.com"
                }
            };

            var viewRenderService = SetupRenderingApi();

            // Assert error is thrown. We should be passing notification, not notification.MatchResEvent. This test should throw an exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => viewRenderService.GenerateMessageContent("DispositionEmail.cshtml", notification.MatchResEvent));

        }
    }
}
