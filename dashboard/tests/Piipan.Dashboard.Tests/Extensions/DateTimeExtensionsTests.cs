using Piipan.Dashboard.Client.Helpers;
using Piipan.Dashboard.Tests.Helpers;
using Xunit;

namespace Piipan.Dashboard.Tests.Extensions
{
    //Because XUnit can run test collections in parallel, tests utilizing FakeLocalTimeZone might stomp on each other.
    //To avoid this, we explicitly put any tests utilizing FakeLocalTimeZone into the same collection to ensure
    //they run serially. https://xunit.net/docs/running-tests-in-parallel
    [Collection("FakeLocalTimeZoneRelatedTests")]
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void TestFakeLocalTimeZone()
        {
            using (new FakeLocalTimeZone(FakeLocalTimeZone.DefaultTestTimeZone))
            {
                // Daylight savings time EST is -5
                Assert.Equal($"1/1/2022 7:00:00 AM TT", FakeLocalTimeZone.Jan_1_2022_Noon_UTC.ToFullTimeWithTimezone());
            }
        }
    }
}
