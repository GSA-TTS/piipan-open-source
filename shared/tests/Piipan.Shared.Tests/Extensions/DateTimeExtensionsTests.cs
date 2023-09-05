using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Piipan.Shared.Extensions;
using Xunit;

namespace Piipan.Shared.Tests.Extensions
{
    /// <summary>
    /// Tests that cover the DateTimeExtensions class
    /// </summary>
    public class DateTimeExtensionsTests
    {
        /// <summary>
        /// Test to make sure the DateTimeExtension returns back the result from the DateTimeProvider's ToEasternTime method.
        /// </summary>
        [Fact]
        public void ToEasternTime_ReturnsDateTime_FromProvider()
        {
            // Arrange
            var mockDate = DateTime.Now;
            Mock<DateTimeProvider> dateTimeProviderMock = new Mock<DateTimeProvider>();
            dateTimeProviderMock.Setup(n => n.ToEasternTime(It.IsAny<DateTime?>())).Returns(mockDate);

            // Act/Assert
            Assert.Equal(mockDate, DateTime.UtcNow.ToEasternTime(dateTimeProviderMock.Object));
        }
    }
}
