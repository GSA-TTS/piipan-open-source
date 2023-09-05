using Piipan.SupportTools.Core.Models;
using Xunit;

namespace Piipan.SupportTools.Core.Tests.Models
{
    public class PoisonMessageDequeuerParamTests
    {
        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var one = new PoisonMessageDequeuerParam
            {
                AccountKey = "1",
                AccountName = "test",
                QueueName = "TT"
            };
            var two = new PoisonMessageDequeuerParam
            {
                AccountKey = one.AccountKey,
                AccountName = one.AccountName,
                QueueName = one.QueueName
            };


            // Act / Assert
            Assert.True(one.QueueName.Equals(two.QueueName));
            Assert.True(one.AccountKey.Equals(two.AccountKey));
            Assert.True(one.AccountName.Equals(two.AccountName));
        }

        [Fact]
        public void Equals_MismatchAllFields()
        {
            // Arrange
            var one = new PoisonMessageDequeuerParam
            {
                AccountKey = "1",
                AccountName = "test",
                QueueName = "TT"
            };
            var two = new PoisonMessageDequeuerParam
            {
                AccountKey = "2",
                AccountName = "test2",
                QueueName = "TT2"
            };


            // Act / Assert
            Assert.False(one.QueueName.Equals(two.QueueName));
            Assert.False(one.AccountKey.Equals(two.AccountKey));
            Assert.False(one.AccountName.Equals(two.AccountName));
        }
    }
}
