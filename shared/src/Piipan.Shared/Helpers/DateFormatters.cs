using System;
using System.Collections.Generic;

namespace Piipan.Shared.Helpers
{
    public class DateFormatters
    {
        /// <summary>
        /// Format a C# List of DateTimes as a string of psql array of datetimes
        /// </summary>
        public static string FormatDatesAsPgArray(List<DateTime> dates) {
            List<string> formattedDateStrings = new List<string>();
            string formatted = "{";
            dates.Sort((x, y) => y.CompareTo(x));
            foreach (var date in dates)
            {
                formattedDateStrings.Add(date.ToString("yyyy-MM-dd"));
            }
            formatted += string.Join(",", formattedDateStrings);
            formatted += "}";
            return formatted;
        }

        public static string RelativeTime(DateTime dtnow, DateTime dtrel)
        {
            // https://stackoverflow.com/a/1248
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            // assumes datetime args are in same timezone.
            // TODO: How can these be normalized?
            var ts = new TimeSpan(dtnow.Ticks - dtrel.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
    }
}
