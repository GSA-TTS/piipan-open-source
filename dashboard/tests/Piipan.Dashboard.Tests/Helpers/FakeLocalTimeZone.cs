using System;
using System.Globalization;
using ReflectionMagic;

namespace Piipan.Dashboard.Tests.Helpers
{
    //Because XUnit can run test collections in parallel, tests utilizing FakeLocalTimeZone might stomp on each other.
    //To avoid this, we should explicitly put any tests utilizing FakeLocalTimeZone into the same collection to ensure
    //they run serially. https://xunit.net/docs/running-tests-in-parallel
    public class FakeLocalTimeZone : IDisposable
    {
        private bool hasBeenDisposed;
        public static TimeZoneInfo DefaultTestTimeZone = TimeZoneInfo.CreateCustomTimeZone("t1", TimeSpan.FromHours(-5), "Testing Time", "Testing Time");
        public static DateTime Jan_1_2022_Noon_UTC = DateTimeOffset.ParseExact("20220101120000+00:00", "yyyyMMddHHmmsszzz", CultureInfo.InvariantCulture).DateTime;

        private readonly TimeZoneInfo _actualLocalTimeZoneInfo;

        private static void SetLocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            typeof(TimeZoneInfo).AsDynamicType().s_cachedData._localTimeZone = timeZoneInfo;
        }

        public FakeLocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            _actualLocalTimeZoneInfo = TimeZoneInfo.Local;
            SetLocalTimeZone(timeZoneInfo);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!hasBeenDisposed)
            {
                if(disposing)
                {
                    //Free managed resources here
                }

                SetLocalTimeZone(_actualLocalTimeZoneInfo);
            }
            hasBeenDisposed = true;
        }

        ~FakeLocalTimeZone() => Dispose(false);
    }
}
