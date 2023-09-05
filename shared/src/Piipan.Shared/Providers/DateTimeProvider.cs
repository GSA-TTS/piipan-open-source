using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Shared.Extensions
{
    /// <summary>
    /// This DateTime Provider provides additional date, time, and timezone functionality to other classes.
    /// </summary>
    public class DateTimeProvider
    {
        /// <summary>
        /// Tries to get a time zone from the passed in ID, returning true if it was found and false if not.
        /// </summary>
        /// <param name="timeZoneId">The ID of the time zone to find.</param>
        /// <param name="timeZoneInfo">The time zone info found, if it exists. If it doesn't exist, it will be null.</param>
        /// <returns></returns>
        public virtual bool TryGetTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return true;
            }
            catch
            {
                timeZoneInfo = null;
                return false;
            }
        }

        /// <summary>
        /// Converts given UTC Time to Eastern Time. If no time provided, it takes the current UTC time.
        /// </summary>
        /// <param name="utcTime">The UTC time to convert</param>
        /// <returns></returns>
        public virtual DateTime ToEasternTime(DateTime? utcTime = null)
        {
            utcTime ??= DateTime.UtcNow;
            TimeZoneInfo timeZoneInfo = null;
            if (!TryGetTimeZoneInfo("Eastern Standard Time", out timeZoneInfo))
            {
                if (!TryGetTimeZoneInfo("America/New_York", out timeZoneInfo))
                {
                    throw new TimeZoneNotFoundException("Eastern time not found");
                }
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, timeZoneInfo);
        }
    }
}
