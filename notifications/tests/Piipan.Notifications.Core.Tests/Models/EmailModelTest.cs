using Piipan.Notifications.Models;
using Xunit;
namespace Piipan.Notifications.Core.Tests.Models
{
    public class EmailModelTest
    {
        [Fact]
        public void Equals_Match()
        {
            // Arrange
            string emails = "test@test.com,test1@test.com";
            var emailone = new EmailModel
            {

                ToList = emails.Split(',').ToList(),
                ToCCList = emails.Split(',').ToList(),
                ToBCCList = emails.Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };
            var emailTwo = emailone;

            // Act / Assert
            Assert.True(emailone.Equals(emailTwo));
        }

        [Fact]
        public void Equals_IdMismatch()
        {
            // Arrange
            string emails = "test@test.com,test1@test.com";
            var emailone = new EmailModel
            {

                ToList = emails.Split(',').ToList(),
                ToCCList = emails.Split(',').ToList(),
                ToBCCList = emails.Split(',').ToList(),
                Body = "Body of the Email",
                Subject = "Subject of the Email",
                From = "noreply@test.com",
            };
            var emailTwo = new EmailModel
            {
                ToList = emailone.ToList,
                ToCCList = emailone.ToCCList,
                ToBCCList = emailone.ToBCCList,
                Body = emailone.Body,
                Subject = emailone.Subject,
                From = "test@test.com"
            };

            // Act / Assert
            Assert.False(emailone.Equals(emailTwo));
        }

    }
}
