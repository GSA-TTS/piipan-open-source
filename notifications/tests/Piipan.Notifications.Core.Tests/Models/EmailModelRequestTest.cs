using Piipan.Notifications.Core.Models;
using Piipan.Notifications.Models;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Models
{
    public class EmailModelRequestTest
    {
        [Fact]
        public void Equals_Match()
        {
            // Arrange
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

            EmailModelRequest request1 = new EmailModelRequest() { Data = emailModel };
            var request2 = request1;

            // Act / Assert
            Assert.True(request1.Equals(request2));
        }

        [Fact]
        public void Equals_IdMismatch()
        {
            // Arrange
            string emails = "test@test.com,test1@test.com";
            var emailModel1 = new EmailModel
            {

                ToList = emails.Split(',').ToList(),
                ToCCList = emails.Split(',').ToList(),
                ToBCCList = emails.Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };
            var emailModel2 = new EmailModel
            {
                ToList = emailModel1.ToList,
                ToCCList = emailModel1.ToCCList,
                ToBCCList = emailModel1.ToBCCList,
                Body = emailModel1.Body,
                Subject = emailModel1.Subject,
                From = "test@test.com"
            };

            EmailModelRequest request1 = new EmailModelRequest() { Data = emailModel1 };
            EmailModelRequest request2 = new EmailModelRequest() { Data = emailModel2 };
            // Act / Assert
            Assert.False(request1.Equals(request2));
        }
    }
}
