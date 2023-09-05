using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Shared.Extensions
{
    /// <summary>
    /// Extends the DateTime class, allowing additional functionality to be added to it.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Allows a UTC Date to be changed to eastern time.
        /// </summary>
        /// <param name="utcTime">The date/time to convert</param>
        /// <param name="dateTimeProvider">The DateTime Provider that will convert the UTC time to Eastern time</param>
        /// <returns></returns>
        public static DateTime ToEasternTime(this DateTime utcTime, DateTimeProvider dateTimeProvider = null)
        {
            dateTimeProvider ??= new DateTimeProvider();
            return dateTimeProvider.ToEasternTime(utcTime);
        }
    }
}
