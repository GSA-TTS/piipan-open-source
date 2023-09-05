using Piipan.Notification.Common.Utilities;
using Xunit;

namespace Piipan.Notifications.Common.Tests.Utilities
{
    public class ValueToDisplayHelperTests
    {
        [Fact]
        public void Validate_Boolean_CorrectOutput()
        {
            Assert.Equal("Yes", ValueToDisplayHelper.GetDisplayValue(true));
            Assert.Equal("No", ValueToDisplayHelper.GetDisplayValue(false));
            Assert.Equal("-", ValueToDisplayHelper.GetDisplayValue((bool?)null));
        }

        [Fact]
        public void Validate_DateTime_CorrectOutput()
        {
            Assert.Equal("09/19/2020", ValueToDisplayHelper.GetDisplayValue(DateTime.Parse("2020-09-19")));
            Assert.Equal("-", ValueToDisplayHelper.GetDisplayValue((DateTime?)null));
        }
    }
}
