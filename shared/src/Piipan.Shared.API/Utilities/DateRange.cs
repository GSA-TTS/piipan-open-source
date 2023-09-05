using Newtonsoft.Json;
using System;

namespace Piipan.Shared.API.Utilities
{
    public class DateRange
    {
        /// <summary>
        /// 	Initializes a new instance of the <see cref="DateRange" /> structure to the specified start and end date.
        /// </summary>
        /// <param name="start">A Datetime that contains that first date in the date range.</param>
        /// <param name="end">A Datetime that contains the last date in the date range.</param>
        /// <remarks>
        ///	Start and end dates are considered inclusive bounds.
        /// </remarks>

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
        public DateRange()
        { }

        /// <summary>
        ///     Gets the start date component of the date range.
        /// </summary>
        [JsonProperty("start")]
        [JsonConverter(typeof(JsonConvertersShared.DateTimeConverter))]
        public DateTime Start { get; set; }


        /// <summary>
        ///     Gets the end date component of the date range.
        /// </summary>
        [JsonProperty("end")]
        [JsonConverter(typeof(JsonConvertersShared.DateTimeConverter))]
        public DateTime End { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DateRange p = obj as DateRange;
            if (p == null)
            {
                return false;
            }

            return
                Start == p.Start &&
                End == p.End;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(
               Start,
               End
            );
        }
    }
}
