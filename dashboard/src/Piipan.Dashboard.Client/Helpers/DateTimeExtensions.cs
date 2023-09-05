namespace Piipan.Dashboard.Client.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToFullTimeWithTimezone(this DateTime dateTime)
        {
            DateTime localDateTime = dateTime.ToLocalTime();
            string tzName = TimeZoneInfo.Local.IsDaylightSavingTime(localDateTime) ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;

            // When running in Web Assembly, the name of the time zone is already abbreviated, but it turns out that's due to a fallback
            // that occurs if the name doesn't exist: https://github.com/dotnet/runtime/pull/45385/files.
            // We should safeguard this in case it changes in the future, and also it will allow our tests to pass since they aren't
            // running in WASM.
            string tzAbbreviation = string.Concat(System.Text.RegularExpressions.Regex
               .Matches(tzName, "[A-Z]")
               .OfType<System.Text.RegularExpressions.Match>()
               .Select(match => match.Value));

            return $"{localDateTime.ToString("M/d/yyyy h:mm:ss tt").ToUpper()} {tzAbbreviation}";
        }
    }
}
